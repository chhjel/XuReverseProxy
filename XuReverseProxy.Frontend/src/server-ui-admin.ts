import VueAppInitializer from "@utils/VueAppInitializer";
import ErrorPage from '@pages/ErrorPage.vue';
import LoginPage from '@pages/LoginPage.vue';
import DashboardPage from '@pages/DashboardPage.vue';
import ManualApprovalProxyAuthPage from '@pages/ManualApprovalProxyAuthPage.vue';

const pages = {
    ErrorPage: ErrorPage,
    LoginPage: LoginPage,
    DashboardPage: DashboardPage,
    ManualApprovalProxyAuthPage: ManualApprovalProxyAuthPage
};

// Init any apps on the current page
new VueAppInitializer().initAppFromElements(pages);
