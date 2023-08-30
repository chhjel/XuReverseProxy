import VueAppInitializer, { InitializableVueApp } from "@utils/VueAppInitializer";
import ErrorPage from '@pages/ErrorPage.vue';
import LoginPage from '@pages/LoginPage.vue';
import AdminApp from '@pages/admin/AdminApp.vue';
import ManualApprovalProxyAuthPage from '@pages/ManualApprovalProxyAuthPage.vue';
import { App } from "vue";
import adminRouter from "./routers/admin-router";

const initializableApps: {[key: string]: InitializableVueApp} = {
    ErrorPage: { component: ErrorPage },
    LoginPage: { component: LoginPage },
    ManualApprovalProxyAuthPage: { component: ManualApprovalProxyAuthPage },
    AdminApp: {
        component: AdminApp,
        onInit: (app) => adminRouterInitializer(app)
    }
};

// Init any apps on the current page
new VueAppInitializer().initAppFromElements(initializableApps);

function adminRouterInitializer(app: App<Element>) {
    app.use(adminRouter);
}
