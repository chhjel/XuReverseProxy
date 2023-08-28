import { createApp } from 'vue'

export default class VueAppInitializer
{
    public initAppFromElements(pages: any): void
    {
        console.log(pages);
        let appElements = Array.from(document.querySelectorAll('[data-vue-component]'));
        appElements.forEach(element => this.initAppFromElement(pages, element));
    }
    
    public initAppFromElement(pages: any, element: Element): void
    {
        let componentName = element.getAttribute("data-vue-component") as any;
        if (componentName == null)
        {
            return;
        }

        let optionsJson = element.getAttribute("data-vue-options");
        let props = {
            options: {}
        };
        if (optionsJson != null) {
            props.options = JSON.parse(optionsJson);
        }

        console.log(`Initializing vue component '${componentName}' with options: `, props);
        let appComponent = (pages as any)[componentName];
        if (appComponent == null) {
            console.error(`Vue component '${componentName}' has not been imported.`);
        }
        
        createApp(appComponent, props)
            .mount(element);
    }
}
