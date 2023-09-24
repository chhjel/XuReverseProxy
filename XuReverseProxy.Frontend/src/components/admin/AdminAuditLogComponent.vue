<script lang="ts">
import { Options } from "vue-class-component";
import { Vue } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import AuditLogService from "@services/AuditLogService";
import { PaginatedResult } from "@generated/Models/Web/PaginatedResult";
import { AdminAuditLogEntry } from "@generated/Models/Core/AdminAuditLogEntry";
import { GetAdminAuditLogEntriesRequestModel } from "@generated/Models/Web/GetAdminAuditLogEntriesRequestModel";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import DateFormats from "@utils/DateFormats";
import PagingComponent from "@components/common/PagingComponent.vue";
import IPDetailsComponent from "./IPDetailsComponent.vue";
import DialogComponent from "@components/common/DialogComponent.vue";
import { htmlAttributeEncode, htmlEncode } from "@utils/HtmlUtils";
import { GlobalVariable } from "@generated/Models/Core/GlobalVariable";
import GlobalVariablesService from "@services/GlobalVariablesService";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    DialogComponent,
    LoaderComponent,
    PagingComponent,
    IPDetailsComponent,
  },
})
export default class AdminAuditLogComponent extends Vue {
  // todo: show filter inputs if no value specified by props. E.g. show audit events for a proxy config on its configpage.
  service: AuditLogService = new AuditLogService();
  currentPageData: PaginatedResult<AdminAuditLogEntry> | null = null;
  filter: GetAdminAuditLogEntriesRequestModel = {
    fromUtc: new Date(new Date().setDate(new Date().getDate() - 1000)),
    toUtc: new Date(new Date().setDate(new Date().getDate() + 1)),
    pageIndex: 0,
    pageSize: 40,
    adminUserId: null,
    proxyConfigId: null,
    clientId: null,
  };

  proxyConfigService: ProxyConfigService = new ProxyConfigService();
  proxyConfigs: Array<ProxyConfig> = [];
  variablesService: GlobalVariablesService = new GlobalVariablesService();
  variables: Array<GlobalVariable> = [];

  async mounted() {
    await this.loadReferencedData();
    await this.loadData();
  }

  async loadReferencedData() {
    this.proxyConfigs = (await this.proxyConfigService.GetAllAsync()).data || [];
    this.variables = (await this.variablesService.GetAllAsync()).data || [];
  }

  async loadData() {
    this.currentPageData = await this.service.GetAdminLogAsync(this.filter);
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  get currentPageItems(): Array<AdminAuditLogEntry> {
    if (this.currentPageData == null) return [];
    else return this.currentPageData.pageItems;
  }

  get totalItemCount(): number {
    return this.currentPageData?.totalItemCount || 0;
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }

  formatDateFull(raw: Date | string): string {
    return DateFormats.dateTimeFull(raw);
  }

  createActionHtml(entry: AdminAuditLogEntry): string {
    let html = htmlEncode(entry.action);

    if (html.includes("[PROXYCONFIG]")) {
      const existing = this.proxyConfigs.find((x) => x.id == entry.relatedProxyConfigId);
      const name = !existing
        ? (htmlEncode(entry.relatedProxyConfigName) || "config") + " (deleted)"
        : htmlEncode(existing.name || entry.relatedProxyConfigName) || "config";
      const nameAtTimeOfLog = entry.relatedProxyConfigName || "<no name>";
      const titlePart =
        existing == null || nameAtTimeOfLog == existing.name
          ? ""
          : `title="Name at event time: ${htmlAttributeEncode(nameAtTimeOfLog)}"`;
      const linkClass = !existing ? "missing" : "";
      html = html.replace(
        "[PROXYCONFIG]",
        `<a href="/#/proxyconfigs/${entry.relatedProxyConfigId}" class="${linkClass}" ${titlePart}>[${name}]</a>`,
      );
    }

    if (html.includes("[CLIENT]")) {
      const name = htmlEncode(entry.relatedClientName || entry.relatedClientId);
      html = html.replace("[CLIENT]", `<a href="/#/client/${entry.relatedClientId}">[${name}]</a>`);
    }

    if (html.includes("[GVAR]")) {
      const existing = this.variables.find((x) => x.id == entry.relatedGlobalVariableId);
      const name = !existing
        ? (htmlEncode(entry.relatedGlobalVariableName) || "variable") + " (deleted)"
        : htmlEncode(existing.name || entry.relatedGlobalVariableName) || "variable";
      const nameAtTimeOfLog = entry.relatedGlobalVariableName || "<no name>";
      const titlePart =
        (existing == null || nameAtTimeOfLog == existing.name)
          ? ""
          : `title="Name at event time: ${htmlAttributeEncode(nameAtTimeOfLog)}"`;
      const linkClass = !existing ? "missing" : "";
      html = html.replace(
        "[GVAR]",
        `<a href="/#/variables/${entry.relatedGlobalVariableId}" class="${linkClass}" ${titlePart}>[${name}]</a>`,
      );
    }

    return html;
  }

  async onPageIndexChanged() {
    await this.loadData();
  }

  ipInDialog: string | null = null;
  ipDialogVisible: boolean = false;
  showIpDialog(ip: string): void {
    this.ipInDialog = ip;
    this.ipDialogVisible = true;
  }
}
</script>

<template>
  <div class="admin-audit-log">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div class="flexbox center-vertical">
        <div class="spacer"></div>
        <button-component
          icon="refresh"
          :disabled="isLoading"
          :inProgress="isLoading"
          title="Refresh"
          iconOnly
          secondary
          @click="loadData"
          class="mr-0"
        ></button-component>
      </div>

      <paging-component
        class="pagination mb-1"
        :count="totalItemCount"
        :pageSize="filter.pageSize"
        v-model:value="filter.pageIndex"
        :disabled="isLoading"
        :asIndex="true"
        :hideIfSinglePage="true"
        @change="onPageIndexChanged"
      />
      <div class="table-wrapper">
        <table>
          <tr>
            <th>When</th>
            <th>IP</th>
            <th>Who</th>
            <th>What</th>
          </tr>
          <tr v-for="item in currentPageItems" :key="item.id" class="item">
            <td class="item__when">
              <code :title="formatDateFull(item.timestampUtc)">{{ formatDate(item.timestampUtc) }}</code>
            </td>
            <td class="item__ip" @click="showIpDialog(item.ip)">
              <code>{{ item.ip }}</code>
            </td>
            <td class="item__who">
              <code :title="item.adminUserId">Admin ({{ item.adminUserId }})</code>
            </td>
            <td class="item__what">
              <div v-html="createActionHtml(item)"></div>
            </td>
          </tr>
        </table>
      </div>

      <paging-component
        class="pagination mt-2"
        :count="totalItemCount"
        :pageSize="filter.pageSize"
        v-model:value="filter.pageIndex"
        :disabled="isLoading"
        :asIndex="true"
        :hideIfSinglePage="true"
        @change="onPageIndexChanged"
      />
    </div>

    <!-- IP Dialog -->
    <dialog-component v-model:value="ipDialogVisible" max-width="800">
      <template #header>IP location</template>
      <template #footer>
        <button-component @click="ipDialogVisible = false" :disabled="isLoading" class="secondary"
          >Close</button-component
        >
      </template>
      <IPDetailsComponent :ip="ipInDialog" :key="ipInDialog || 'empty'" />
    </dialog-component>
  </div>
</template>

<style scoped lang="scss">
.admin-audit-log {
  .table-wrapper {
    overflow-x: auto;
  }

  table {
    text-align: left;
    width: 100%;
  }
  th {
    padding: 3px;
  }
  td {
    color: var(--color--text-dark);
    padding: 3px;
  }
  tr {
    border-bottom: 1px solid var(--color--text-darker);
    padding: 3px;

    &:nth-child(odd) {
      background-color: var(--color--table-odd);
    }

    &:not(:first-child) {
      /* cursor: pointer; */
      &:hover {
        background-color: var(--color--secondary-darken);
      }
    }
  }

  .item {
    &__when {
      width: 1%;
      overflow: hidden;
      text-overflow: ellipsis;
      padding: 5px;
    }

    &__ip {
      cursor: pointer;
      width: 1%;
    }

    &__who {
      width: 1%;
      width: 90px;
      max-width: 90px;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    &__what {
      overflow: hidden;
      text-overflow: ellipsis;
    }

    :deep(a.missing) {
      color: var(--color--danger-lighten);
    }
  }
}
</style>
