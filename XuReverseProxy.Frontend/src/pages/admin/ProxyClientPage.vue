<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import StringUtils from "@utils/StringUtils";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import MapComponent from "@components/common/MapComponent.vue";
import { LoadStatus } from "@services/ServiceBase";
import ProxyAuthenticationDataService from "@services/ProxyAuthenticationDataService";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import { GenericResult } from "@generated/Models/Web/GenericResult";
import GlobeComponent from "@components/common/GlobeComponent.vue";
import IPDetailsComponent from "@components/admin/IPDetailsComponent.vue";
import { ProxyClientIdentitySolvedChallengeData } from "@generated/Models/Core/ProxyClientIdentitySolvedChallengeData";
import { createProxyAuthenticationSummary } from "@utils/ProxyAuthenticationDataUtils";
import DateFormats from "@utils/DateFormats";
import ClientAuditLogComponent from "@components/admin/ClientAuditLogComponent.vue";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";

interface SolvedClientData {
  proxyConfig: ProxyConfig;
  auth: ProxyAuthenticationData;
  solvedData: ProxyClientIdentitySolvedChallengeData;
}

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    CheckboxComponent,
    CodeInputComponent,
    MapComponent,
    GlobeComponent,
    LoaderComponent,
    IPDetailsComponent,
    ClientAuditLogComponent,
  },
})
export default class ProxyClientPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: ProxyClientIdentityService = new ProxyClientIdentityService();
  proxyConfigService: ProxyConfigService = new ProxyConfigService();
  proxyAuthService: ProxyAuthenticationDataService = new ProxyAuthenticationDataService();
  // proxyAuthConditionService: ConditionDataService = new ConditionDataService();
  statuses: Array<LoadStatus> = [this.service.status, this.proxyConfigService.status, this.proxyAuthService.status];
  client: ProxyClientIdentity | null = null;
  clientId: string = "";

  isBlocked: boolean = false;
  clientBlockedNote: string = "";
  clientNote: string = "";

  proxyConfigs: Array<ProxyConfig> = [];
  proxyConfigAuths: Array<ProxyAuthenticationData> = [];

  clientAuthData: Array<SolvedClientData> = [];

  async mounted() {
    this.clientId = StringUtils.firstOrDefault(this.$route.params.clientId);
    const result = await this.service.GetAsync(this.clientId);
    if (!result.success) {
      console.error(result.message);
    } else {
      this.client = result.data;
      this.onClientLoaded();
    }
  }

  async onClientLoaded(): Promise<any> {
    this.isBlocked = this.client.blocked;
    this.clientBlockedNote = this.client.blockedMessage;
    this.clientNote = this.client.note;

    this.proxyConfigs = (await this.proxyConfigService.GetAllAsync())?.data || [];
    this.proxyConfigAuths = (await this.proxyAuthService.GetAllAsync())?.data || [];
    this.client.solvedChallenges.forEach((x) => {
      const auth = this.proxyConfigAuths.find((a) => a.id == x.authenticationId);
      const proxyConfig = this.proxyConfigs.find((c) => c.id == auth?.proxyConfigId);
      if (auth && proxyConfig) {
        this.clientAuthData.push({
          proxyConfig: proxyConfig,
          auth: auth,
          solvedData: x,
        });
      }
    });
  }

  get manualApprovalsInProgress(): Array<any> {
    let items: Array<any> = [];
    this.proxyConfigAuths.forEach((auth) => {
      if (auth.challengeTypeId == "ProxyChallengeTypeManualApproval") {
        const config = this.proxyConfigs.find((x) => x.id == auth.proxyConfigId);
        if (config == null) return;

        const authData = this.client.data.filter((d) => d.authenticationId == auth.id);
        if (!authData.some((x) => x.key == "requested" && x.value == "true")) return;
        else if (this.client.solvedChallenges.some((x) => x.authenticationId == auth.id && x.solvedId == auth.solvedId))
          return;

        const label = `Awaiting manual approval [${authData.find((x) => x.key == "easyCode")?.value}]`;
        const link = `/proxyAuth/approve/${this.client.id}/${auth.proxyConfigId}/${auth.id}/${auth.solvedId}`;
        items.push({
          proxyConfig: config,
          label: label,
          link: link,
        });
      } else if (auth.challengeTypeId == "ProxyChallengeTypeOTP") {
        const config = this.proxyConfigs.find((x) => x.id == auth.proxyConfigId);
        if (config == null) return;

        const authData = this.client.data.filter((d) => d.authenticationId == auth.id);
        if (!authData.some((x) => x.key == "code")) return;
        else if (this.client.solvedChallenges.some((x) => x.authenticationId == auth.id && x.solvedId == auth.solvedId))
          return;

        const label = `Awaiting OTP input [${authData.find((x) => x.key == "code")?.value}]`;
        items.push({
          proxyConfig: config,
          label: label,
          link: "",
        });
      }
    });
    return items;
  }

  get isLoading(): boolean {
    return this.statuses.some((x) => x.inProgress);
  }

  async updateClientNote(): Promise<any> {
    await this.service.SetClientNoteAsync({
      clientId: this.client.id,
      note: this.clientNote,
    });
  }

  async toggleClientBlocked(): Promise<any> {
    const oldNode = this.clientBlockedNote;
    const oldValue = this.isBlocked;

    if (!this.isBlocked) {
      const note = prompt("Note (optional)");
      if (note === null) return;
      this.clientBlockedNote = note;
    } else {
      this.clientBlockedNote = null;
    }

    this.isBlocked = !this.isBlocked;
    const result = await this.updateClientBlocked();
    if (result?.success !== true) {
      this.isBlocked = oldValue;
      this.clientBlockedNote = oldNode;
    }
  }

  async updateClientBlocked(): Promise<GenericResult> {
    return await this.service.SetClientBlockedAsync({
      clientId: this.client.id,
      message: this.clientBlockedNote || "",
      blocked: this.isBlocked,
    });
  }

  createAuthSummary(auth: ProxyAuthenticationData): string {
    return createProxyAuthenticationSummary(auth);
  }

  formatDate(raw: Date | string): string {
    return DateFormats.defaultDateTime(raw);
  }
}
</script>

<template>
  <div class="proxyclient-page">
    <loader-component :status="statuses" v-if="!service.status.hasDoneAtLeastOnce" />

    <div v-if="client">
      <!-- Metadata -->
      <div class="block overflow-x-scroll mb-4 meta">
        <div>
          IP: <code>{{ client.ip }}</code>
        </div>
        <div v-if="client.userAgent">
          UserAgent: <code>{{ client.userAgent }}</code>
        </div>
        <div>
          Created at: <code> {{ formatDate(client.createdAtUtc) }}</code>
        </div>
        <div v-if="client.lastAccessedAtUtc">
          Last accessed: <code>{{ formatDate(client.lastAccessedAtUtc) }}</code>
        </div>
        <div v-if="client.lastAttemptedAccessedAtUtc">
          Last attempted accessed:
          <code>{{ formatDate(client.lastAttemptedAccessedAtUtc) }}</code>
        </div>

        <checkbox-component
          label="Blocked"
          offLabel="Not blocked"
          :value="isBlocked"
          class="mt-2 mb-2"
          warnColorOn
          @click="toggleClientBlocked"
          :disabled="isLoading"
        />
        <div v-if="clientBlockedNote" class="mt-2">
          Blocked note: <code>{{ clientBlockedNote }}</code>
        </div>
      </div>

      <!-- Notes -->
      <div class="block mb-4 pt-2">
        <div class="block-title">Client note</div>
        <code-input-component
          v-model:value="clientNote"
          language=""
          class="mt-2"
          height="100px"
          :wordWrap="true"
          ref="noteEditor"
          :readOnly="isLoading"
          @blur="updateClientNote"
        />
      </div>

      <!-- IP Details -->
      <IPDetailsComponent :ip="client.ip" :key="client.ip || 'empty'" :relatedClientId="clientId" />

      <!-- Challenges in progress -->
      <div class="block overflow-x-scroll mb-4" v-if="manualApprovalsInProgress.length > 0">
        <div class="block-title mb-2">Challenges in progress</div>
        <div v-for="approval in manualApprovalsInProgress">
          <router-link :to="`/proxyconfigs/${approval.proxyConfig?.id}`">
            <b>{{ approval.proxyConfig?.name }}</b>
          </router-link>
          -
          <a :href="approval.link" v-if="approval.link">{{ approval.label }}</a>
          <span v-if="!approval.link">{{ approval.label }}</span>
        </div>
      </div>

      <!-- Solved data -->
      <div class="block overflow-x-scroll mb-4" v-if="clientAuthData.length > 0">
        <div class="block-title mb-2">Solved challenges</div>
        <div v-for="solvedConfig in clientAuthData">
          <router-link :to="`/proxyconfigs/${solvedConfig.proxyConfig?.id}`">
            <b>{{ solvedConfig.proxyConfig?.name }}</b>
          </router-link>
          <span> - {{ createAuthSummary(solvedConfig.auth) }}</span>
          <span v-if="solvedConfig?.solvedData?.solvedAtUtc">
            - Completed at:
            {{ formatDate(solvedConfig?.solvedData?.solvedAtUtc) }}</span
          >
          <span v-if="solvedConfig?.solvedData?.solvedId != solvedConfig?.auth?.solvedId">
            - SolvedId was changed.</span
          >
        </div>
      </div>

      <!-- Audit log -->
      <div class="block overflow-x-scroll mb-4">
        <div class="block-title mb-2">Log</div>
        <client-audit-log-component :clientId="clientId" class="audit-log" />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.proxyclient-page {
  padding-top: 20px;

  .meta {
    font-size: 12px;
  }

  .audit-log {
    position: relative;
    z-index: 1;
    margin-top: -44px;
  }

  .ipdetails-location {
    display: flex;
    flex-wrap: wrap;

    .ipdetails-flag {
      position: relative;
      top: 1px;
      width: 22px;
      max-height: 18px;
      margin-right: 3px;
    }

    .ipdetails-location__part {
      white-space: nowrap;

      &.flag-part {
        display: flex;
      }

      &:not(:last-child) {
        &::after {
          content: ">";
          font-size: 15px;
          display: inline-block;
          margin: 0 5px;
        }
      }
    }
  }

  .globe {
    height: 300px;
  }
}
</style>
