<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import DateFormats from "@utils/DateFormats";
import { ProxyClientIdentitiesPagedRequestModel } from "@generated/Models/Web/ProxyClientIdentitiesPagedRequestModel";
import { PaginatedResult } from "@generated/Models/Web/PaginatedResult";
import PagingComponent from "@components/common/PagingComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
    PagingComponent,
  },
})
export default class ProxyClientsPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: ProxyClientIdentityService = new ProxyClientIdentityService();

  currentPageData: PaginatedResult<ProxyClientIdentity> | null = null;
  filter: ProxyClientIdentitiesPagedRequestModel = {
    pageIndex: 0,
    pageSize: 40,
  };

  async mounted() {
    await this.loadData();
  }

  async loadData() {
    this.currentPageData = await this.service.GetPagedAsync(this.filter);
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  get currentPageItems(): Array<ProxyClientIdentity> {
    if (this.currentPageData == null) return [];
    else return this.currentPageData.pageItems;
  }

  get totalItemCount(): number {
    return this.currentPageData?.totalItemCount || 0;
  }

  navToClient(clientId: string, newTab: boolean = false): void {
    if (newTab) window.open(`/#/client/${clientId}`, "_blank");
    else this.$router.push({ name: "client", params: { clientId: clientId } });
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }

  async onPageIndexChanged() {
    await this.loadData();
  }
}
</script>

<template>
  <div class="proxyclients-page">
    <loader-component :status="service.status" v-if="!service.status.hasDoneAtLeastOnce || !service.status.success" />

    <div v-if="service.status.hasDoneAtLeastOnce">
      <div class="flexbox center-vertical">
        <p>Note: Clients are only created for proxies with authentication.</p>
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
            <th>Note</th>
            <th>IP</th>
            <th style="font-size: 12px">Last access</th>
            <th style="font-size: 12px">Last attempted access</th>
            <th>Status</th>
            <th>UserAgent</th>
          </tr>
          <tr
            v-for="client in currentPageItems"
            :key="client.id"
            class="client"
            @click="navToClient(client.id)"
            @click.middle="navToClient(client.id, true)"
          >
            <td class="client__note">
              <code :title="client.note">{{ client.note }}</code>
            </td>
            <td class="client__ip">
              <code :title="client.ip">{{ client.ip }}</code>
            </td>
            <td class="client__la">
              <code v-if="client.lastAccessedAtUtc">{{ formatDate(client.lastAccessedAtUtc) }}</code>
            </td>
            <td class="client__laa">
              <code v-if="client.lastAttemptedAccessedAtUtc">{{ formatDate(client.lastAttemptedAccessedAtUtc) }}</code>
            </td>
            <td class="client__meta">
              <code v-if="client.blocked">Blocked</code>
            </td>
            <td class="client__useragent">
              <code :title="client.userAgent">{{ client.userAgent }}</code>
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
  </div>
</template>

<style scoped lang="scss">
.proxyclients-page {
  padding-top: 20px;

  .table-wrapper {
    overflow-x: auto;
  }

  table {
    text-align: left;
    width: 100%;
  }
  th {
    padding: 3px;
    white-space: nowrap;
  }
  td {
    color: var(--color--text-dark);
    padding: 4px;
  }
  tr {
    border-bottom: 1px solid var(--color--text-darker);
    padding: 3px;

    &:nth-child(odd) {
      background-color: var(--color--table-odd);
    }

    &:not(:first-child) {
      cursor: pointer;
      &:hover {
        background-color: var(--color--secondary);
      }
    }
  }

  .client {
    &__useragent {
      width: 300px;
      max-width: 300px;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    &__ip {
      cursor: pointer;
      width: 1%;
    }
    &__la,
    &__laa {
      /* width: 150px;
			max-width: 150px; */
      width: 1%;
    }
    &__meta {
      width: 1%;
    }
    &__note {
      width: 1%;
      max-width: 300px;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  }
}
</style>
