import VueAppInitializer, { InitializableVueApp } from "@utils/VueAppInitializer";
import ErrorPage from "@pages/ErrorPage.vue";
import ProxyChallengePage from "@pages/ProxyChallengePage.vue";

const initializableApps: { [key: string]: InitializableVueApp } = {
  ErrorPage: { component: ErrorPage },
  ProxyChallengePage: { component: ProxyChallengePage },
};

// Init any apps on the current page
new VueAppInitializer().initAppFromElements(initializableApps);
