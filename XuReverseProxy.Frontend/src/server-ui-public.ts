import VueAppInitializer from "@utils/VueAppInitializer";
import ErrorPage from '@pages/ErrorPage.vue';
import ProxyChallengePage from '@pages/ProxyChallengePage.vue';

const pages = {
    ErrorPage: ErrorPage,
    ProxyChallengePage: ProxyChallengePage
};

// Init any apps on the current page
new VueAppInitializer().initAppFromElements(pages);
