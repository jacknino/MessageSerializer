# Declare module of your plugin under Jekyll module
module Jekyll
    module CustomFilter
        # Each method of the module creates a custom Jekyll filter
        def link_url(input)
            return "/#{input}.html"
        end

        def make_link(input)
           return "[#{input}](/#{input}.html)"
        end
    end
end

Liquid::Template.register_filter(Jekyll::CustomFilter)
