<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import { EmptyGuid } from "@utils/Constants";
import { createProxyConfigResultingProxyUrl } from "@utils/ProxyConfigUtils";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import { ProxyConfigMode } from "@generated/Enums/Core/ProxyConfigMode";
import ServerConfigService from "@services/ServerConfigService";
import { SortByThenBy } from "@utils/SortUtils";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
  },
})
export default class ProxyConfigsPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  proxyConfigService: ProxyConfigService = new ProxyConfigService();
  proxyConfigs: Array<ProxyConfig> = [];
  globalProxyEnabled: boolean | null = null;

  async mounted() {
    const result = await this.proxyConfigService.GetAllFullAsync();
    if (!result.success) {
      console.error(result.message);
    }
    this.proxyConfigs = result.data || [];

    this.globalProxyEnabled = await new ServerConfigService().IsConfigFlagEnabledAsync("EnableForwarding");
  }

  async addNewProxyConfig() {
    let config: ProxyConfig = {
      id: EmptyGuid,
      enabled: false,
      authentications: [],
      name: "New Proxy Config",
      challengeDescription: "",
      port: null,
      subdomain: "",
      challengeTitle: "",
      mode: ProxyConfigMode.Forward,
      destinationPrefix: "http://host.docker.internal:",
      staticHTML: "",
      showCompletedChallenges: true,
      showChallengesWithUnmetRequirements: true,
      conditionsNotMetMessage: '',
      showConditionsNotMet: true,
      proxyConditions: []
    };
    const result = await this.proxyConfigService.CreateOrUpdateAsync(config);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      config = result.data;
      this.proxyConfigs.push(config);
      this.$router.push({
        name: "proxyconfig",
        params: { configId: config.id },
      });
    }
  }

  get sortedConfigs(): Array<ProxyConfig> {
    return this.proxyConfigs.sort((a, b) =>
      SortByThenBy(
        a,
        b,
        (x) => x.enabled,
        (x) => x.name,
        (a, b) => <any>b.enabled - <any>a.enabled,
        (a, b) => a.name?.localeCompare(b.name),
      ),
    );
  }

  getConfigStatus(config: ProxyConfig): string {
    if (this.globalProxyEnabled === false) return "(forwarding disabled in server config)";
    else if (!config.enabled) return "(disabled)";
    else "";
  }

  getConfigIcon(config: ProxyConfig): any {
    if (!config.enabled) return "do_disturb_on";
    else if (config.authentications.length > 0) return "vpn_lock";
    else return "public";
  }

  getConfigIconClasses(config: ProxyConfig): any {
    let classes: any = {};
    if (!config.enabled || this.globalProxyEnabled === false) classes["disabled"] = true;
    if (config.authentications.length) classes["has-auth"] = true;
    return classes;
  }

  getResultingProxyUrl(config: ProxyConfig): string {
    return createProxyConfigResultingProxyUrl(
      config,
      this.options.serverScheme,
      this.options.serverPort,
      this.options.serverDomain,
    );
  }

  configModeIsForward(config: ProxyConfig): boolean {
    return config.mode == ProxyConfigMode.Forward;
  }
  configModeIsStaticHTML(config: ProxyConfig): boolean {
    return config.mode == ProxyConfigMode.StaticHTML;
  }
}
</script>

<template>
  <div class="proxyconfigs-page">
    <loader-component :status="proxyConfigService.status" />
    <div v-if="proxyConfigService.status.hasDoneAtLeastOnce">
      <div v-if="sortedConfigs.length == 0 && proxyConfigService.status.done">- No proxied configured yet -</div>
      <div v-for="config in sortedConfigs" :key="config.id">
        <router-link :to="{ name: 'proxyconfig', params: { configId: config.id } }" class="proxyconfig">
          <div class="proxyconfig__header">
            <div class="material-icons icon" :class="getConfigIconClasses(config)">
              {{ getConfigIcon(config) }}
            </div>
            <div class="proxyconfig__name">
              {{ config.name }}
              <span class="proxyconfig__status">{{ getConfigStatus(config) }}</span>
            </div>
          </div>
          <div class="proxyconfig__forwardsummary" v-if="configModeIsForward(config)">
            <code>{{ getResultingProxyUrl(config) }}</code>
            <span class="ml-2 mr-2">forwards to</span>
            <code>{{ config.destinationPrefix }}</code>
          </div>
          <div class="proxyconfig__forwardsummary" v-if="configModeIsStaticHTML(config)">
            <code>{{ getResultingProxyUrl(config) }}</code>
            <span class="ml-2">serves static HTML</span>
          </div>
        </router-link>
      </div>
      <button-component @click="addNewProxyConfig" v-if="proxyConfigService.status.done" class="primary ml-0"
        >Add new config</button-component
      >
    </div>
  </div>
</template>

<style scoped lang="scss">
.proxyconfigs-page {
  padding-top: 20px;

  .proxyconfig {
    display: block;
    padding: 5px;
    margin: 5px 0;

    &:hover {
      text-decoration: none;
      background-color: var(--color--hover-bg);
    }

    &__header {
      display: flex;
      align-items: center;
    }

    &__status {
      font-size: 12px;
      color: var(--color--text-darker);
    }

    &__forwardsummary {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      font-size: 12px;
      color: var(--color--secondary);
      margin-left: 31px;
    }

    .icon {
      width: 24px;
      margin-right: 5px;
      color: var(--color--primary-lighten);

      &.disabled {
        color: var(--color--warning-base);
      }
    }
  }
}
</style>
