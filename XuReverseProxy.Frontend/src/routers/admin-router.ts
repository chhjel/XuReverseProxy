import { RouteRecordRaw, createRouter, createWebHashHistory } from 'vue-router'
import Admin404Page from '@pages/admin/Admin404Page.vue';
import AdminOverviewPage from '@pages/admin/AdminOverviewPage.vue';
import ProxyConfigsPage from '@pages/admin/ProxyConfigsPage.vue';
import ProxyConfigPage from '@pages/admin/ProxyConfigPage.vue';
import ServerConfigPage from '@pages/admin/ServerConfigPage.vue';
import ProxyClientsPage from '@pages/admin/ProxyClientsPage.vue';
import ProxyClientPage from '@pages/admin/ProxyClientPage.vue';
import ScheduledTasksPage from '@pages/admin/ScheduledTasksPage.vue';
import AdminAuditLogPage from '@pages/admin/AdminAuditLogPage.vue';

const routes: Readonly<RouteRecordRaw[]> = [
    { name: "/", path: "/", component: AdminOverviewPage },
    { name: "proxyconfigs", path: "/proxyconfigs", component: ProxyConfigsPage },
    { name: "proxyconfig", path: "/proxyconfigs/:configId", component: ProxyConfigPage },
    { name: "serverconfig", path: "/serverconfig", component: ServerConfigPage },
    { name: "clients", path: "/clients", component: ProxyClientsPage },
    { name: "client", path: "/client/:clientId", component: ProxyClientPage },
    { name: "jobs", path: "/jobs", component: ScheduledTasksPage },
    { name: "admin-audit-log", path: "/admin-audit-log", component: AdminAuditLogPage },
    { name: "404", path: "/:catchAll(.*)", component: Admin404Page },
]

const adminRouter = createRouter({
    history: createWebHashHistory(),
    routes: routes
  });
export default adminRouter;
