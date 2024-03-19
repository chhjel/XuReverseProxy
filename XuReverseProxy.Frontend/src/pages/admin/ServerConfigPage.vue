<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ServerConfigService from "@services/ServerConfigService";
import { RuntimeServerConfigItem } from "@generated/Models/Core/RuntimeServerConfigItem";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import { ClientBlockedHtmlPlaceholders, Html404Placeholders, IPBlockedHtmlPlaceholders, PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import LoaderComponent from "@components/common/LoaderComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    CheckboxComponent,
    CodeInputComponent,
    ExpandableComponent,
    PlaceholderInfoComponent,
    LoaderComponent,
  },
})
export default class ServerConfigPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  service: ServerConfigService = new ServerConfigService();
  runtimeConfigs: Array<RuntimeServerConfigItem> = [];

  allowLoader: boolean | null = null;

  async mounted() {
    await this.loadConfig();
  }

  async loadConfig() {
    const result = await this.service.GetAllAsync();
    if (!result.success) {
      console.error(result.message);
    } else {
      this.allowLoader = false;
    }
    this.runtimeConfigs = result.data || null;
  }

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  getConfigBool(key: string) {
    const item = this.runtimeConfigs.find((x) => x.key == key);
    return item?.value == "true";
  }

  async toggleConfig(key: string) {
    if (this.isLoading) return;

    const item = this.runtimeConfigs.find((x) => x.key == key);
    if (item == null) return;

    const oldValue = item.value;
    const newValue = !JSON.parse(item.value.toLowerCase());
    item.value = `${newValue}`;

    const result = await this.service.CreateOrUpdateAsync(item);
    if (!result.success) item.value = oldValue;
  }

  getConfigString(key: string) {
    const item = this.runtimeConfigs.find((x) => x.key == key);
    return item?.value || "";
  }

  async saveConfig(key: string, value: string, loaderId: string) {
    const item = this.runtimeConfigs.find((x) => x.key == key);
    if (item == null) return;

    const oldValue = item.value;
    item.value = value;

    const result = await this.service.CreateOrUpdateAsync(item, null, loaderId);
    if (!result.success) item.value = oldValue;
  }
}
</script>

<template>
  <div class="serverconfig-page">
    <loader-component :status="service.status" :value="allowLoader" />
    
    <div class="runtime-config" v-if="service.status.hasDoneAtLeastOnce">
      <div class="block">
        <checkbox-component
          label="Proxy server enabled"
          offLabel="Proxy server disabled"
          :disabled="isLoading"
          :value="getConfigBool('EnableForwarding')"
          @click="toggleConfig('EnableForwarding')"
          class="mt-2 mb-2"
        />

        <checkbox-component
          label="Notifications enabled"
          offLabel="Notifications disabled"
          :disabled="isLoading"
          :value="getConfigBool('EnableNotifications')"
          @click="toggleConfig('EnableNotifications')"
          class="mt-2 mb-2"
        />

        <checkbox-component
          label="Require admin login for manual approval page"
          offLabel="Require admin login for manual approval page"
          :disabled="isLoading"
          :value="getConfigBool('EnableManualApprovalPageAuthentication')"
          @click="toggleConfig('EnableManualApprovalPageAuthentication')"
          class="mt-2 mb-2"
        />

        <checkbox-component
          label="Enable memory logging for debugging"
          offLabel="Enable memory logging for debugging"
          :disabled="isLoading"
          :value="getConfigBool('EnableMemoryLogging')"
          @click="toggleConfig('EnableMemoryLogging')"
          class="mt-2 mb-2"
        />
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.serverconfig-page {
  padding-top: 20px;
}
</style>
