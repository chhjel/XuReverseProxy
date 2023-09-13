import VueAppInitializer, { InitializableVueApp } from "@utils/VueAppInitializer";
import ErrorPage from '@pages/ErrorPage.vue';
import LoginPage from '@pages/LoginPage.vue';
import AdminApp from '@pages/admin/AdminApp.vue';
import ManualApprovalProxyAuthPage from '@pages/ManualApprovalProxyAuthPage.vue';
import { App } from "vue";
import createAdminRouter from "./routers/admin-router";

const initializableApps: {[key: string]: InitializableVueApp} = {
    ErrorPage: { component: ErrorPage },
    LoginPage: { component: LoginPage },
    ManualApprovalProxyAuthPage: { component: ManualApprovalProxyAuthPage },
    AdminApp: {
        component: AdminApp,
        onInit: (app, opts) => adminRouterInitializer(app, opts)
    }
};

// Init any apps on the current page
new VueAppInitializer().initAppFromElements(initializableApps);

function adminRouterInitializer(app: App<Element>, opts: any) {
    app.use(createAdminRouter(opts));
}
