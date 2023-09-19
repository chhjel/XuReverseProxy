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
import ClientAuditLogPage from '@pages/admin/ClientAuditLogPage.vue';
import ServerLogPage from '@pages/admin/ServerLogPage.vue';
import NotificationsPage from '@pages/admin/NotificationsPage.vue';
import NotificationPage from '@pages/admin/NotificationPage.vue';
import BlockedIPsPage from '@pages/admin/BlockedIPsPage.vue';
import BlockedIPPage from '@pages/admin/BlockedIPPage.vue';

import { nextTick } from 'vue';

const routes: Readonly<RouteRecordRaw[]> = [
    { name: "/", path: "/", meta: { title: '' }, component: AdminOverviewPage },
    { name: "proxyconfigs", path: "/proxyconfigs", meta: { title: 'Proxy Configs' }, component: ProxyConfigsPage },
    { name: "proxyconfig", path: "/proxyconfigs/:configId", meta: { title: 'Proxy Config' }, component: ProxyConfigPage },
    { name: "serverconfig", path: "/serverconfig", meta: { title: 'Server Config' }, component: ServerConfigPage },
    { name: "clients", path: "/clients", meta: { title: 'Clients' }, component: ProxyClientsPage },
    { name: "client", path: "/client/:clientId", meta: { title: 'Client' }, component: ProxyClientPage },
    { name: "notifications", path: "/notifications", meta: { title: 'Notifications' }, component: NotificationsPage },
    { name: "notification", path: "/notification/:notificationId", meta: { title: 'Notification' }, component: NotificationPage },
    { name: "blocked-ips", path: "/blocked-ips", meta: { title: 'Blocked IPs' }, component: BlockedIPsPage },
    { name: "blocked-ip", path: "/blocked-ip/:blockedId", meta: { title: 'Blocked IP' }, component: BlockedIPPage },
    { name: "jobs", path: "/jobs", meta: { title: 'Jobs' }, component: ScheduledTasksPage },
    { name: "admin-audit-log", path: "/admin-audit-log", meta: { title: 'Admin Audit Log' }, component: AdminAuditLogPage },
    { name: "client-audit-log", path: "/client-audit-log", meta: { title: 'Client Audit Log' }, component: ClientAuditLogPage },
    { name: "server-log", path: "/server-log", meta: { title: 'Server Log' }, component: ServerLogPage },
    { name: "404", path: "/:catchAll(.*)", meta: { title: '404' }, component: Admin404Page },
]

const adminRouter = createRouter({
    history: createWebHashHistory(),
    routes: routes
  });

export default function createAdminRouter(opts: any) {
  const serverName = <string>opts.serverName || 'XuReverseProxy';
  // Allow whitespace as servername to remove suffix
  const titleSuffix = serverName.trim().length == 0 ? '' : ` | ${serverName}`;

  adminRouter.afterEach((to, from) => {
      nextTick(() => {
          const pageName = <string>to.meta.title || '';
          // If pagename is not configured => use servername only
          const title = pageName.trim().length == 0 ? serverName : `${pageName}${titleSuffix}`;
          document.title = title;
      });
  });
  
  return adminRouter;
}
