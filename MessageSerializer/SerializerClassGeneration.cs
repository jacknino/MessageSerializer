using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MessageSerializer
{
    public class SerializerClassGeneration : CodeGenerationBase
    {
        protected static readonly string TypedObjectFieldName = "typedObject";
        protected List<ITypeSelector> _typeSelectors;

        public SerializerClassGeneration(List<ITypeSelector> typeSelectors)
            : base("SerializerBase")
        {
            _typeSelectors = typeSelectors;
        }

        public SerializerBase CreateSerializerClassForType(MessageSerializedClassInfo classInfo)
        {
            Type type = classInfo.ClassType;
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            GenerateCodeFromType(type, compileUnit, classInfo);
            CompilerResults results = CompileCode(type, compileUnit);

            Type createdType = results.CompiledAssembly.GetType(string.Format(type.Namespace + "." + GetClassName(type)), true, true);
            SerializerBase serializerBase = (SerializerBase)Activator.CreateInstance(createdType, new object[] { classInfo });

            return serializerBase;
        }

        protected void GenerateCodeFromType(Type type, CodeCompileUnit codeCompileUnit, MessageSerializedClassInfo classInfo)
        {
            AddReferences(codeCompileUnit, type);

            CodeNamespace codeNamespace = CreateUsingStatements(type.Namespace, codeCompileUnit);

            CreateCodeForClass(codeNamespace, type, classInfo);
        }

        protected void AddReferences(CodeCompileUnit compileUnit, Type type)
        {
            compileUnit.ReferencedAssemblies.Add("System.dll");
            compileUnit.ReferencedAssemblies.Add(Path.Combine(MyDirectory, "MessageSerializer.dll"));
            compileUnit.ReferencedAssemblies.Add(type.Module.FullyQualifiedName);
        }

        protected CodeNamespace CreateUsingStatements(string codeNamespaceName, CodeCompileUnit codeCompileUnit)
        {
            CodeNamespace codeNamespace = new CodeNamespace(codeNamespaceName);
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("MessageSerializer"));

            codeCompileUnit.Namespaces.Add(codeNamespace);
            return codeNamespace;
        }

        protected void CreateCodeForClass(CodeNamespace codeNamespace, Type type, MessageSerializedClassInfo classInfo)
        {
            CodeTypeDeclaration classDeclaration = CreateClassDeclaration(type, codeNamespace);

            classDeclaration.Members.AddRange(CreateMemberVariables(classInfo));
            classDeclaration.Members.Add(CreateConstructor(classInfo));
            classDeclaration.Members.Add(CreateSerializeMethod(classInfo));
            classDeclaration.Members.Add(CreateDeserializeMethod(classInfo));
            classDeclaration.Members.Add(CreateToStringMethod(classInfo));
        }

        protected CodeTypeMemberCollection CreateMemberVariables(MessageSerializedClassInfo classInfo)
        {
            CodeTypeMemberCollection codeTypeMemberCollection = new CodeTypeMemberCollection();

            // Create TypeSerializers
            foreach (MessageSerializedPropertyInfo propertyInfo in classInfo.Properties)
            {
                string memberVariableName = GetSerializerMemberVariableName(propertyInfo);
                Type fieldType = GetFieldType(propertyInfo);

                codeTypeMemberCollection.Add(CreateField(memberVariableName, fieldType));
            }

            // Create Calculators
            foreach (CalculatedFieldInfo calculatedFieldInfo in classInfo.CalculatedFields)
            {
                string memberVariableName = GetCalculatorMemberVariableName(calculatedFieldInfo);
                Type fieldType = calculatedFieldInfo.CalculatorType;
                codeTypeMemberCollection.Add(CreateField(memberVariableName, fieldType));
            }

            return codeTypeMemberCollection;
        }

        protected CodeMemberField CreateField(string variableName, Type fieldType)
        {
            CodeMemberField field = new CodeMemberField();
            field.Attributes = MemberAttributes.Family;
            field.Name = variableName;
            field.Type = new CodeTypeReference(fieldType);
            return field;
        }

        protected CodeConstructor CreateConstructor(MessageSerializedClassInfo classInfo)
        {
            string classInfoVariableName = "classInfo";
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(MessageSerializedClassInfo), classInfoVariableName));

            // new the TypeSerializers
            for (int index = 0; index < classInfo.Properties.Count; ++index)
            {
                MessageSerializedPropertyInfo propertyInfo = classInfo.Properties[index];
                Type fieldType = GetFieldType(propertyInfo);

                // _serializerFieldName = new TypeSerializerNumeric<Type>(classInfo.Properties[index]);
                CodeAssignStatement createSerializer = new CodeAssignStatement(
                    new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                    new CodeObjectCreateExpression(
                        fieldType,
                        new CodeArrayIndexerExpression(
                            new CodePropertyReferenceExpression(
                                new CodeVariableReferenceExpression(classInfoVariableName),
                                "Properties"),
                            new CodePrimitiveExpression(index))));
                constructor.Statements.Add(createSerializer);
            }

            // new the Calculators
            foreach(CalculatedFieldInfo calculatedFieldInfo in classInfo.CalculatedFields)
            {
                // _calculatorFieldName = this.CreateCalculator<CalculatorType, ResultType>(name, classInfo);
                CodeAssignStatement createCalculator = new CodeAssignStatement(
                    new CodeVariableReferenceExpression(GetCalculatorMemberVariableName(calculatedFieldInfo)),
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(
                            new CodeThisReferenceExpression(),
                            "CreateCalculator",
                            new CodeTypeReference[]
                            {
                                new CodeTypeReference(calculatedFieldInfo.CalculatorType),
                                new CodeTypeReference(calculatedFieldInfo.CalculatorResultPropertyInfo.ElementType),
                            }),
                        new CodeExpression[]
                        {
                            new CodePrimitiveExpression(calculatedFieldInfo.Name),
                            new CodeVariableReferenceExpression(classInfoVariableName)
                        }));

                constructor.Statements.Add(createCalculator);
            }

            return constructor;
        }

        protected CodeMemberMethod CreateSerializeMethod(MessageSerializedClassInfo classInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.Name = "Serialize";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "objectToSerialize"));
            method.ReturnType = new CodeTypeReference(typeof(byte[]));

            CodeVariableDeclarationStatement typedObjectDeclaration = new CodeVariableDeclarationStatement(
                new CodeTypeReference(classInfo.ClassType),
                TypedObjectFieldName,
                new CodeCastExpression(
                    new CodeTypeReference(classInfo.ClassType),
                    new CodeVariableReferenceExpression("objectToSerialize")));
            method.Statements.Add(typedObjectDeclaration);

            List<string> arrayNames = new List<string>();
            foreach (MessageSerializedPropertyInfo propertyInfo in classInfo.Properties)
            {
                string arrayName = GetArrayName(propertyInfo);
                arrayNames.Add(arrayName);

                if (propertyInfo.IsCalculatedResult)
                    continue;

                // Now get the actual assignment statement
                // For blob length fields we have to wait until we've figured out the length before we can serialize
                if (propertyInfo.MessagePropertyAttribute.BlobType != BlobTypes.Length)
                {
                    method.Statements.AddRange(GetConvertToByteArrayStatement(arrayName, propertyInfo));
                }

                // Since we got the data now we need to set the blob length and serialize it
                if (propertyInfo.MessagePropertyAttribute.BlobType == BlobTypes.Data)
                {
                    method.Statements.AddRange(CreateBlobLengthStatements(arrayName, propertyInfo, classInfo.Properties));
                }
            }

            method.Statements.AddRange(GetCalculatedFieldStatements(classInfo, arrayNames));

            // Add call to combine method to put all the arrays together
            CodeExpression[] combineParameters = new CodeExpression[arrayNames.Count];
            for (int index = 0; index < arrayNames.Count; ++index)
            {
                combineParameters[index] = new CodeVariableReferenceExpression(arrayNames[index]);
            }

            CodeVariableDeclarationStatement combineStatement = new CodeVariableDeclarationStatement(
                typeof(byte[]),
                "serialized",
                new CodeMethodInvokeExpression(
                    new CodeTypeReferenceExpression(typeof(ArrayOps)),
                    "Combine",
                    combineParameters));
            method.Statements.Add(combineStatement);

            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("serialized")));
            return method;
        }

        protected CodeStatementCollection GetCalculatedFieldStatements(MessageSerializedClassInfo classInfo, List<string> arrayNames)
        {
            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();

            // This is to have a placeholder for instances where the variable has already been declared
            // so that it should not be declared again when doing the actual assignment.  This is related
            // to when two calculated fields depend on each other in some way.
            HashSet<int> alreadyDeclaredIndexes = new HashSet<int>();

            for (int currentIndex = 0; currentIndex < classInfo.CalculatedFields.Count; ++currentIndex)
            {
                var calculatedFieldInfo = classInfo.CalculatedFields[currentIndex];
                string arrayName = GetArrayName(calculatedFieldInfo.CalculatorResultPropertyInfo);

                // If the calculated field is included in the calculation for some reason then 
                // then we need to declare the array and get the bytes right now.  This also holds
                // true for any calculated fields that have a later priority but are included in
                // this calculation.  An example where this situation could happen is if there is
                // a length field in the message and the length includes a CRC-16 field at the end
                // of the message. The length needs to include the length of the CRC.  In this
                // scenario, the calculation for the CRC might need to include the length which
                // would need to be the correct value before the calculation can be done so the length
                // needs to have a lower priority to make sure it is calculated first.  Where this
                // would be invalid would be if the CRC was a variable length field but that doesn't
                // logically make sense anyways.
                for (int followingIndex = currentIndex; followingIndex < classInfo.CalculatedFields.Count; ++followingIndex)
                {
                    var followingCalculatedFieldInfo = classInfo.CalculatedFields[followingIndex];
                    if (calculatedFieldInfo.IncludedPropertyIndexes.Contains(followingCalculatedFieldInfo.CalculatedResultIndex) && !alreadyDeclaredIndexes.Contains(followingCalculatedFieldInfo.CalculatedResultIndex))
                    {
                        codeStatementCollection.AddRange(GetConvertToByteArrayStatement(GetArrayName(followingCalculatedFieldInfo.CalculatorResultPropertyInfo), followingCalculatedFieldInfo.CalculatorResultPropertyInfo));
                        alreadyDeclaredIndexes.Add(followingIndex);
                    }
                }

                CodeExpression[] calculateArrayArguments = new CodeExpression[calculatedFieldInfo.IncludedPropertyIndexes.Count];
                int arrayArgumentIndex = 0;
                foreach(int includedPropertyIndex in calculatedFieldInfo.IncludedPropertyIndexes)
                {
                    calculateArrayArguments[arrayArgumentIndex++] = new CodeVariableReferenceExpression(arrayNames[includedPropertyIndex]);
                }

                CodePropertyReferenceExpression propertyReferenceExpression = new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(TypedObjectFieldName), calculatedFieldInfo.CalculatorResultPropertyInfo.PropertyInfo.Name);

                // typedObject.FieldName = _calculator.Calculate(array1, array2, ...)
                CodeAssignStatement calculateAuthenticationStatement = new CodeAssignStatement(
                    propertyReferenceExpression,
                    new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression(GetCalculatorMemberVariableName(calculatedFieldInfo)),
                        "Calculate",
                        calculateArrayArguments));

                codeStatementCollection.Add(calculateAuthenticationStatement);

                if (!alreadyDeclaredIndexes.Contains(currentIndex))
                {
                    // We need to declare the array now
                    codeStatementCollection.AddRange(GetConvertToByteArrayStatement(arrayName, calculatedFieldInfo.CalculatorResultPropertyInfo));
                }
                else
                {
                    // We already declared the array above
                    // arrayFieldName = _serializer.Serialize(typedObject.FieldName);
                    CodeAssignStatement assignBytesAgainStatement = new CodeAssignStatement(
                        new CodeVariableReferenceExpression(arrayName),
                        GetAssignmentExpressionForByteArray(calculatedFieldInfo.CalculatorResultPropertyInfo, propertyReferenceExpression));
                    codeStatementCollection.Add(assignBytesAgainStatement);
                }
            }

            return codeStatementCollection;
        }

        protected string GetClassName(Type type)
        {
            return _baseClassName + type.Name;
        }

        protected Type GetFieldType(MessageSerializedPropertyInfo propertyInfo)
        {
            foreach (ITypeSelector typeSelector in _typeSelectors)
            {
                Type fieldType = typeSelector.CheckType(propertyInfo);
                if (fieldType != null)
                    return fieldType;
            }

            throw new Exception($"Don't know what TypeSerializer to use for property {propertyInfo.PropertyInfo.Name} of type {propertyInfo.ElementType.FullName}");
        }

        protected string GetSerializerMemberVariableName(MessageSerializedPropertyInfo propertyInfo)
        {
            return "_serializer" + propertyInfo.PropertyInfo.Name;
        }

        protected string GetCalculatorMemberVariableName(CalculatedFieldInfo calculatedFieldInfo)
        {
            return "_calculator" + calculatedFieldInfo.Name;
        }

        protected static string GetArrayName(MessageSerializedPropertyInfo propertyInfo)
        {
            return "array" + propertyInfo.PropertyInfo.Name;
        }

        protected static string GetListBytesName(CalculatedFieldInfo calculatedFieldInfo)
        {
            return "listBytes" + calculatedFieldInfo.Name;
        }

        protected CodeStatementCollection GetConvertToByteArrayStatement(string arrayName, MessageSerializedPropertyInfo propertyInfo)
        {
            CodeVariableReferenceExpression objectToSerializeExpression = new CodeVariableReferenceExpression(TypedObjectFieldName);
            CodePropertyReferenceExpression propertyReferenceExpression = new CodePropertyReferenceExpression(objectToSerializeExpression, propertyInfo.PropertyInfo.Name);

            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();

            CodeExpression assignmentExpression = GetAssignmentExpressionForByteArray(propertyInfo, propertyReferenceExpression);
            CodeVariableDeclarationStatement expression = new CodeVariableDeclarationStatement(
                typeof(byte[]),
                arrayName,
                assignmentExpression);
            codeStatementCollection.Add(expression);

            return codeStatementCollection;
        }

        protected CodeExpression GetAssignmentExpressionForByteArray(MessageSerializedPropertyInfo propertyInfo, CodePropertyReferenceExpression propertyReferenceExpression)
        {
            // _serializerWhatever.Serialize(typedObject.Whatever);
            CodeExpression assignmentExpression = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                "Serialize",
                new CodeExpression[]
                {
                    propertyReferenceExpression
                });

            return assignmentExpression;
        }

        protected CodeStatementCollection CreateBlobLengthStatements(string blobDataArrayName, MessageSerializedPropertyInfo blobDataPropertyInfo, List<MessageSerializedPropertyInfo> properties)
        {
            // First we need to get the property info for the length property
            // Then we need to assign the length to the size of the blobArray
            // Then we need to serialize the length
            MessageSerializedPropertyInfo blobLengthPropertyInfo = GetPropertyInfo(properties, blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty);
            string blobLengthArrayName = GetArrayName(blobLengthPropertyInfo);

            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();
            CodeAssignStatement assignLength = new CodeAssignStatement(
                new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(TypedObjectFieldName),
                    blobLengthPropertyInfo.PropertyInfo.Name),
                new CodeCastExpression(
                    blobLengthPropertyInfo.ElementType,
                    new CodePropertyReferenceExpression(
                        new CodeVariableReferenceExpression(blobDataArrayName),
                        "Length")));
            codeStatementCollection.Add(assignLength);

            codeStatementCollection.AddRange(GetConvertToByteArrayStatement(blobLengthArrayName, blobLengthPropertyInfo));
            return codeStatementCollection;
        }

        protected MessageSerializedPropertyInfo GetPropertyInfo(List<MessageSerializedPropertyInfo> properties, string propertyName)
        {
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyInfo.Name == propertyName)
                    return propertyInfo;
            }

            throw new Exception(string.Format("Couldn't find a property with name {0}", propertyName));
        }

        protected CodeMemberMethod CreateDeserializeMethod(MessageSerializedClassInfo classInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod();

            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.Name = "Deserialize";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(byte[]), "bytes"));

            CodeParameterDeclarationExpression currentArrayParameterDeclarationExpression = new CodeParameterDeclarationExpression(typeof(int), "currentArrayIndex");
            currentArrayParameterDeclarationExpression.Direction = FieldDirection.Ref;
            method.Parameters.Add(currentArrayParameterDeclarationExpression);
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DeserializeStatus), "status"));

            method.ReturnType = new CodeTypeReference(typeof(object));

            // If there are calculated fields that have the Verify flag set we need to create a List<byte[]>
            // for each of them to put the bytes that should be used for the calculation for the verification
            method.Statements.AddRange(GetCreateListBytesVariables(classInfo));

            // ClassType typedObject = new ClassType();
            CodeVariableDeclarationStatement typedObjectDeclaration = new CodeVariableDeclarationStatement(
                new CodeTypeReference(classInfo.ClassType),
                TypedObjectFieldName,
                new CodeObjectCreateExpression(classInfo.ClassType));
            method.Statements.Add(typedObjectDeclaration);

            CodeVariableReferenceExpression currentArrayIndexExpression = new CodeVariableReferenceExpression("currentArrayIndex");

            foreach (MessageSerializedPropertyInfo propertyInfo in classInfo.Properties)
            {
                if (!propertyInfo.IsVariableLength)
                {
                    method.Statements.AddRange(GetAssignFromByteArrayStatement(classInfo, propertyInfo, currentArrayIndexExpression, GetPropertyLengthExpression(propertyInfo)));
                }
                else if (propertyInfo.MessagePropertyAttribute.BlobType == BlobTypes.Data)
                {
                    method.Statements.AddRange(GetAssignDataFromBlobByteArrayStatement(propertyInfo, currentArrayIndexExpression, classInfo.Properties, classInfo));
                }
                else
                {
                    method.Statements.AddRange(GetAssignStatementForVariableLengthField(propertyInfo, classInfo));
                }
            }

            // If we have any calculated fields that need to be verified now is the time to call the Verify method
            method.Statements.AddRange(GetVerifyCalculatedFieldStatements(classInfo));

            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(TypedObjectFieldName)));
            return method;
        }

        protected virtual CodeStatementCollection GetCreateListBytesVariables(MessageSerializedClassInfo classInfo)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            foreach (CalculatedFieldInfo calculatedFieldInfo in classInfo.CalculatedFields)
            {
                if (calculatedFieldInfo.CalculatorResultAttribute.Verify)
                {
                    // List<byte[]> listBytesName = new List<byte[]>();
                    CodeVariableDeclarationStatement arrayDeclaration = new CodeVariableDeclarationStatement(
                        new CodeTypeReference(typeof(List<byte[]>)),
                        GetListBytesName(calculatedFieldInfo),
                        new CodeObjectCreateExpression(typeof(List<byte[]>)));
                    statements.Add(arrayDeclaration);
                }
            }

            return statements;
        }

        protected virtual CodeStatementCollection GetVerifyCalculatedFieldStatements(MessageSerializedClassInfo classInfo)
        {
            CodeStatementCollection statements = new CodeStatementCollection();
            foreach (CalculatedFieldInfo calculatedFieldInfo in classInfo.CalculatedFields)
            {
                if (calculatedFieldInfo.CalculatorResultAttribute.Verify)
                {
                    // _calculator.Verify(status, typedObject.CalculatorField, array1, array2, ...);
                    var verifyExpression = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression(GetCalculatorMemberVariableName(calculatedFieldInfo)),
                        "Verify",
                        new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("status"),
                            new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(TypedObjectFieldName), calculatedFieldInfo.CalculatorResultPropertyInfo.PropertyInfo.Name),
                            new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(GetListBytesName(calculatedFieldInfo)), "ToArray"), 
                        });

                    statements.Add(verifyExpression);
                }
            }

            return statements;
        }

        protected CodeExpression GetPropertyLengthExpression(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.ElementIsMessageSerializableObject)
            {
                // Serializer.Instance.GetFixedLength<ElementType>()
                return new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Serializer)), "Instance"), 
                        "GetFixedLength",
                        new CodeTypeReference[]
                        {
                            new CodeTypeReference(propertyInfo.ElementType) 
                        }));
            }

            return new CodePrimitiveExpression(propertyInfo.MessagePropertyAttribute.Length);
        }

        protected CodeStatementCollection GetAssignStatementForVariableLengthField(MessageSerializedPropertyInfo variableLengthFieldPropertyInfo, MessageSerializedClassInfo classInfo)
        {
            CalculatedFieldInfo messageLengthCalculatedFieldInfo = classInfo.GetCalculatedLengthInfo();
            if (messageLengthCalculatedFieldInfo == null)
                throw new Exception($"Class {classInfo.ClassType.FullName}, property {variableLengthFieldPropertyInfo.PropertyInfo.Name} is a variable length non-blob field but there isn't a related calculated length field");

            CodeVariableReferenceExpression currentArrayIndexExpression = new CodeVariableReferenceExpression("currentArrayIndex");

            IEnumerable<int> blobLengthIndexes = messageLengthCalculatedFieldInfo.GetAssociatedBlobLengthFieldIndexes(classInfo.Properties);
            var getVaryingLengthFieldLengthArguments = new List<CodeExpression>();
            // (int)typedObject.LengthFieldName
            getVaryingLengthFieldLengthArguments.Add(
                new CodeCastExpression(
                    typeof(int),
                    new CodePropertyReferenceExpression(
                        new CodeVariableReferenceExpression(TypedObjectFieldName),
                        messageLengthCalculatedFieldInfo.CalculatorResultPropertyInfo.PropertyInfo.Name)));
            foreach (int blobLengthIndex in blobLengthIndexes)
            {
                // (int)typedObject.BlobLengthFieldName
                getVaryingLengthFieldLengthArguments.Add(
                    new CodeCastExpression(
                        typeof(int),
                        new CodePropertyReferenceExpression(
                            new CodeVariableReferenceExpression(TypedObjectFieldName),
                            classInfo.Properties[blobLengthIndex].PropertyInfo.Name)));
            }

            // _calculatorLength.GetVaryingLengthFieldLength((int)typedObject.LengthFieldName, (int)typedObject.BlobLength1, (int)typedObject.BlobLength2)
            CodeMethodInvokeExpression variableLengthFieldLengthExpression = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression(GetCalculatorMemberVariableName(messageLengthCalculatedFieldInfo)),
                "GetVaryingLengthFieldLength",
                getVaryingLengthFieldLengthArguments.ToArray());

            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();
            if (variableLengthFieldPropertyInfo.IsList)
                codeStatementCollection.AddRange(AssignListDataFromByteArrayStatement(classInfo, variableLengthFieldPropertyInfo, currentArrayIndexExpression, variableLengthFieldLengthExpression));
            else
                codeStatementCollection.AddRange(GetAssignFromByteArrayStatement(classInfo, variableLengthFieldPropertyInfo, currentArrayIndexExpression, variableLengthFieldLengthExpression));

            return codeStatementCollection;
        }

        protected CodeStatementCollection GetAssignDataFromBlobByteArrayStatement(MessageSerializedPropertyInfo blobDataPropertyInfo, CodeVariableReferenceExpression currentArrayIndexExpression, List<MessageSerializedPropertyInfo> properties, MessageSerializedClassInfo classInfo)
        {
            MessageSerializedPropertyInfo blobLengthPropertyInfo = GetPropertyInfo(properties, blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty);
            CodeExpression fieldLengthExpression = new CodePropertyReferenceExpression(
                new CodeVariableReferenceExpression(TypedObjectFieldName),
                blobLengthPropertyInfo.PropertyInfo.Name);

            if (blobDataPropertyInfo.IsList)
                return AssignListDataFromByteArrayStatement(classInfo, blobDataPropertyInfo, currentArrayIndexExpression, fieldLengthExpression);

            return GetAssignFromByteArrayStatement(classInfo, blobDataPropertyInfo, currentArrayIndexExpression, fieldLengthExpression);
        }

        List<CalculatedFieldInfo> GetCalculatedFieldsThatIncludeThisFieldAndRequireVerification(IEnumerable<CalculatedFieldInfo> calculatedFields, MessageSerializedPropertyInfo propertyInfo)
        {
            return calculatedFields.Where(item => item.CalculatorResultAttribute.Verify && item.IncludedPropertyIndexes.Contains(propertyInfo.Index)).ToList();
        }

        protected CodeStatementCollection AssignListDataFromByteArrayStatement(MessageSerializedClassInfo classInfo, MessageSerializedPropertyInfo propertyInfo, CodeVariableReferenceExpression currentArrayIndexExpression, CodeExpression fieldLengthExpression)
        {
            CodeVariableReferenceExpression objectToSerializeExpression = new CodeVariableReferenceExpression(TypedObjectFieldName);
            CodePropertyReferenceExpression propertyReferenceExpression = new CodePropertyReferenceExpression(objectToSerializeExpression, propertyInfo.PropertyInfo.Name);

            List<CalculatedFieldInfo> calculatedFieldsThatIncludeThisField = GetCalculatedFieldsThatIncludeThisFieldAndRequireVerification(classInfo.CalculatedFields, propertyInfo);

            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();

            CodeExpression assignmentExpression;
            if (calculatedFieldsThatIncludeThisField.Count == 0)
            {
                // _serializerWhatever.DeserializeList<List<Type>>(bytes, ref currentArrayIndex, length, status);
                assignmentExpression = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                        "DeserializeList",
                        new CodeTypeReference[]
                        {
                            new CodeTypeReference(propertyInfo.PropertyInfo.PropertyType)
                        }),
                        new CodeExpression[]
                        {
                            new CodeVariableReferenceExpression("bytes"),
                            new CodeDirectionExpression(FieldDirection.Ref, currentArrayIndexExpression),
                            fieldLengthExpression,
                            new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("status"))
                        });
            }
            else
            {
                List<CodeExpression> argumentsToDeserialize = new List<CodeExpression>()
                {
                    new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                    new CodeVariableReferenceExpression("bytes"),
                    new CodeDirectionExpression(FieldDirection.Ref, currentArrayIndexExpression),
                    fieldLengthExpression,
                    new CodeVariableReferenceExpression("status")
                };
                argumentsToDeserialize.AddRange(calculatedFieldsThatIncludeThisField.Select(item => new CodeVariableReferenceExpression(GetListBytesName(item))));

                // this.DeserializeList<List<Type>>(_serializerWhatever, bytes, ref currentArrayIndex, length, status, array1, array2);
                assignmentExpression = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeThisReferenceExpression(), 
                        "DeserializeList",
                        new CodeTypeReference[]
                        {
                            new CodeTypeReference(propertyInfo.PropertyInfo.PropertyType),
                            new CodeTypeReference(propertyInfo.ElementType)
                        }),
                        argumentsToDeserialize.ToArray());
            }

            codeStatementCollection.Add(new CodeAssignStatement(propertyReferenceExpression, assignmentExpression));
            return codeStatementCollection;
        }

        protected CodeStatementCollection GetAssignFromByteArrayStatement(MessageSerializedClassInfo classInfo, MessageSerializedPropertyInfo propertyInfo, CodeVariableReferenceExpression currentArrayIndexExpression, CodeExpression fieldLengthExpression)
        {
            CodePropertyReferenceExpression propertyReferenceExpression = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(TypedObjectFieldName), propertyInfo.PropertyInfo.Name);

            List<CalculatedFieldInfo> calculatedFieldsThatIncludeThisField = GetCalculatedFieldsThatIncludeThisFieldAndRequireVerification(classInfo.CalculatedFields, propertyInfo);

            CodeStatementCollection codeStatementCollection = new CodeStatementCollection();

            CodeExpression assignmentExpression;
            if (calculatedFieldsThatIncludeThisField.Count == 0)
            {
                // If there aren't any calculated fields that depend on this field then typeSerializer.Deserialize can just be called directly
                //assignmentExpression = GetDeserializeTypeExpression(propertyInfo, currentArrayIndexExpression, fieldLengthExpression);
                // _serializerWhatever.Deserialize(bytes, ref currentArrayIndex, length, ref status);
                assignmentExpression = new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                    "Deserialize",
                    new CodeExpression[]
                    {
                        new CodeVariableReferenceExpression("bytes"),
                        new CodeDirectionExpression(FieldDirection.Ref, currentArrayIndexExpression),
                        fieldLengthExpression,
                        new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("status"))
                    });
            }
            else
            {
                // Otherwise we need to call SerializerBase.Deserialize with the arrays that need to have the bytes from this field added
                // so that the calculated field can be recalculated to verify the received value matches.
                // this.Deserialize<TResultType>(_serializerWhatever, bytes, ref currentArrayIndex, length, status, array1, array2);
                List<CodeExpression> argumentsToDeserialize = new List<CodeExpression>()
                {
                    new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                    new CodeVariableReferenceExpression("bytes"),
                    new CodeDirectionExpression(FieldDirection.Ref, currentArrayIndexExpression),
                    fieldLengthExpression,
                    //new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("status")),
                    new CodeVariableReferenceExpression("status")
                };
                argumentsToDeserialize.AddRange(calculatedFieldsThatIncludeThisField.Select(item => new CodeVariableReferenceExpression(GetListBytesName(item))));

                assignmentExpression = new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(),
                    "Deserialize",
                    argumentsToDeserialize.ToArray());
            }

            codeStatementCollection.Add(new CodeAssignStatement(propertyReferenceExpression, assignmentExpression));

            return codeStatementCollection;
        }

        //protected CodeExpression GetDeserializeTypeExpression(MessageSerializedPropertyInfo propertyInfo, CodeVariableReferenceExpression currentArrayIndexExpression, CodeExpression fieldLengthExpression)
        //{
        //    // _serializerWhatever.Deserialize(bytes, ref currentArrayIndex, length, ref status);
        //    CodeExpression assignmentExpression = new CodeMethodInvokeExpression(
        //        new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
        //        "Deserialize",
        //        new CodeExpression[]
        //        {
        //            new CodeVariableReferenceExpression("bytes"),
        //            new CodeDirectionExpression(FieldDirection.Ref, currentArrayIndexExpression),
        //            fieldLengthExpression,
        //            new CodeDirectionExpression(FieldDirection.Ref, new CodeVariableReferenceExpression("status"))
        //        });

        //    return assignmentExpression;
        //}

        protected CodeTypeDeclaration CreateClassDeclaration(Type type, CodeNamespace codeNamespace)
        {
            CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(GetClassName(type));
            codeTypeDeclaration.BaseTypes.Add(new CodeTypeReference("SerializerBase"));
            codeNamespace.Types.Add(codeTypeDeclaration);
            return codeTypeDeclaration;
        }

        protected CodeTypeMember CreateToStringMethod(MessageSerializedClassInfo classInfo)
        {
            CodeMemberMethod method = new CodeMemberMethod();

            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            method.Name = "ToString";
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "objectToPrint"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "indentLevel"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ToStringFormatProperties), "formatProperties"));

            method.ReturnType = new CodeTypeReference(typeof(string));

            CodeVariableDeclarationStatement typedObjectDeclaration = new CodeVariableDeclarationStatement(
                new CodeTypeReference(classInfo.ClassType),
                TypedObjectFieldName,
                new CodeCastExpression(
                    new CodeTypeReference(classInfo.ClassType),
                    new CodeVariableReferenceExpression("objectToPrint")));
            method.Statements.Add(typedObjectDeclaration);

            CodeVariableDeclarationStatement stringResultExpression = new CodeVariableDeclarationStatement(
                typeof(string),
                "stringResult",
                new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Empty"));
            method.Statements.Add(stringResultExpression);

            bool isFirstProperty = true;
            foreach (MessageSerializedPropertyInfo propertyInfo in classInfo.Properties)
            {
                CodePropertyReferenceExpression propertyExpression = new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(TypedObjectFieldName),
                    propertyInfo.PropertyInfo.Name);

                method.Statements.Add(CreateAddToStringResultStatement(CreateToStringStatementForProperty(propertyExpression, propertyInfo.PropertyInfo.Name, propertyInfo, isFirstProperty)));

                isFirstProperty = false;
            }

            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("stringResult")));
            return method;
        }

        protected CodeStatement CreateAddToStringResultStatement(CodeExpression stringToAddExpression)
        {
            return new CodeAssignStatement(
                new CodeVariableReferenceExpression("stringResult"),
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("stringResult"),
                    CodeBinaryOperatorType.Add,
                    stringToAddExpression));
        }

        protected CodeExpression CreateToStringStatementForProperty(CodeExpression elementExpression, string nameToUse, MessageSerializedPropertyInfo propertyInfo, bool isFirstProperty)
        {
            //_serializerWhatever.ToString(typedObject.Whatever, indentLevel, formatProperties, isFirstProperty));
            CodeMethodInvokeExpression toStringExpression = new CodeMethodInvokeExpression(
                new CodeVariableReferenceExpression(GetSerializerMemberVariableName(propertyInfo)),
                "ToString",
                new CodeExpression[]
                {
                        elementExpression,
                        new CodeVariableReferenceExpression("indentLevel"),
                        new CodeVariableReferenceExpression("formatProperties"),
                        new CodePrimitiveExpression(isFirstProperty)
                });

            return toStringExpression;
        }
    }
}
