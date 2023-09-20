import { App, DefineComponent, createApp } from "vue";

export interface InitializableVueApp {
  component: DefineComponent<object, object, any>;
  onInit?: (app: App<Element>, options: any) => void;
}

export default class VueAppInitializer {
  public initAppFromElements(initializableApps: { [key: string]: InitializableVueApp }): void {
    const appElements = Array.from(document.querySelectorAll("[data-vue-component]"));
    appElements.forEach((element) => this.initAppFromElement(initializableApps, element));
  }

  public initAppFromElement(initializableApps: { [key: string]: InitializableVueApp }, element: Element): void {
    const componentName = element.getAttribute("data-vue-component");
    if (componentName == null) {
      return;
    }

    const optionsJson = element.getAttribute("data-vue-options");
    const props = {
      options: {},
    };
    if (optionsJson != null) {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
      props.options = JSON.parse(optionsJson);
    }

    console.log(`Initializing vue component '${componentName}' with options: `, props);
    const initializableApp = initializableApps[componentName];
    if (initializableApp == null) {
      console.error(`Vue component '${componentName}' has not been imported.`);
    }

    const app = createApp(initializableApp.component, props);
    if (initializableApp.onInit != null) initializableApp.onInit(app, props.options);
    app.mount(element);
  }
}
