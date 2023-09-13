import { App, DefineComponent, createApp } from 'vue'

export interface InitializableVueApp {
    component: DefineComponent<{}, {}, any>;
    onInit?: (app: App<Element>, options: any) => void;
}

export default class VueAppInitializer
{
    public initAppFromElements(initializableApps: {[key: string]: InitializableVueApp}): void
    {
        let appElements = Array.from(document.querySelectorAll('[data-vue-component]'));
        appElements.forEach(element => this.initAppFromElement(initializableApps, element));
    }
    
    public initAppFromElement(initializableApps: {[key: string]: InitializableVueApp}, element: Element): void
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
        let initializableApp = initializableApps[componentName];
        if (initializableApp == null) {
            console.error(`Vue component '${componentName}' has not been imported.`);
        }
        
        const app = createApp(initializableApp.component, props);
        if (initializableApp.onInit != null) initializableApp.onInit(app, props.options);
        app.mount(element);
    }
}
