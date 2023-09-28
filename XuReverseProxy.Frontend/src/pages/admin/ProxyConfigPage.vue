<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import ProxyConfigService from "@services/ProxyConfigService";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import StringUtils from "@utils/StringUtils";
import ProxyAuthenticationDataService from "@services/ProxyAuthenticationDataService";
import ConditionDataService from "@services/ConditionDataService";
import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import DialogComponent from "@components/common/DialogComponent.vue";
import ProxyConfigEditor from "@components/admin/proxyConfig/ProxyConfigEditor.vue";
import ProxyAuthenticationDataEditor from "@components/admin/proxyConfig/ProxyAuthenticationDataEditor.vue";
import { ConditionData } from "@generated/Models/Core/ConditionData";
import { createProxyAuthenticationSummary } from "@utils/ProxyAuthenticationDataUtils";
import { createConditionDataSummary } from "@utils/ConditionDataUtils";
import IdUtils from "@utils/IdUtils";
import { EmptyGuid, ProxyAuthChallengeTypeOptions } from "@utils/Constants";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import draggable from "vuedraggable";
import { ProxyAuthenticationDataOrderData } from "@generated/Models/Web/ProxyAuthenticationDataOrderData";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import ConditionDatasEditor from "@components/admin/proxyConfig/ConditionDatasEditor.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    CheckboxComponent,
    AdminNavMenu,
    DialogComponent,
    ProxyConfigEditor,
    ProxyAuthenticationDataEditor,
    draggable,
    LoaderComponent,
    ConditionDatasEditor,
  },
})
export default class ProxyConfigPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  proxyConfigService: ProxyConfigService = new ProxyConfigService();
  proxyAuthService: ProxyAuthenticationDataService = new ProxyAuthenticationDataService();

  proxyConfig: ProxyConfig | null = null;
  notFound: boolean = false;
  authDialogVisible: boolean = false;
  authInDialog: ProxyAuthenticationData | null = null;
  emptyGuid: string = EmptyGuid;
  allowInitialLoader: boolean | null = null;

  async mounted() {
    await this.loadConfig();
  }

  async loadConfig() {
    const configId = StringUtils.firstOrDefault(this.$route.params.configId);
    const result = await this.proxyConfigService.GetAsync(configId);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.allowInitialLoader = false;
    }
    this.proxyConfig = result.data || null;
    if (this.proxyConfig)
      this.proxyConfig.authentications = this.proxyConfig.authentications.sort(({ order: a }, { order: b }) => a - b);
  }

  // Note: only saves config itself, not auths etc
  async saveConfig() {
    const result = await this.proxyConfigService.CreateOrUpdateAsync(this.proxyConfig);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.proxyConfig = result.data;
      if (this.proxyConfig)
        this.proxyConfig.authentications = this.proxyConfig.authentications.sort(({ order: a }, { order: b }) => a - b);
    }
  }

  async deleteConfig() {
    if (!confirm("Delete this proxy config?")) return;

    const result = await this.proxyConfigService.DeleteAsync(this.proxyConfig.id);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.$router.push({ name: "proxyconfigs" });
    }
  }

  async saveAuth() {
    const result = await this.proxyAuthService.CreateOrUpdateAsync(this.authInDialog);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      this.authInDialog = result.data;
      this.authDialogVisible = false;
      this.onAuthSaved(result.data);
    }
  }

  async deleteAuth() {
    const result = await this.proxyAuthService.DeleteAsync(this.authInDialog.id);
    if (!result.success) {
      console.error(result.message);
      alert(result.message);
    } else {
      const index = this.proxyConfig.authentications.findIndex((x) => x.id == this.authInDialog.id);
      if (index != -1) this.proxyConfig.authentications.splice(index, 1);
      this.authDialogVisible = false;
      this.authInDialog = null;
    }
  }

  showAuthDialog(auth: ProxyAuthenticationData) {
    this.authInDialog = auth;
    this.authDialogVisible = true;
  }

  onAuthSaved(auth: ProxyAuthenticationData) {
    const index = this.proxyConfig.authentications.findIndex((x) => x.id == auth.id);
    if (index == -1) this.proxyConfig.authentications.push(auth);
    else this.proxyConfig.authentications[index] = auth;
  }

  createAuthSummary(auth: ProxyAuthenticationData): string {
    return createProxyAuthenticationSummary(auth);
  }

  async onAddAuthClicked(): Promise<any> {
    let order = 0;
    if (this.proxyConfig.authentications && this.proxyConfig.authentications.length > 0) {
      order = this.proxyConfig.authentications.reduce((prev, cur) => Math.max(prev, cur.order), 0);
    }
    const auth: ProxyAuthenticationData = {
      id: EmptyGuid,
      challengeTypeId: ProxyAuthChallengeTypeOptions[0].typeId,
      challengeJson: "{}",
      proxyConfig: null,
      proxyConfigId: this.proxyConfig.id,
      solvedDuration: null,
      solvedId: IdUtils.generateId(),
      order: order,
      conditions: [],
    };
    this.showAuthDialog(auth);
  }

  get isLoading(): boolean {
    return this.proxyConfigService.status.inProgress || this.proxyAuthService.status.inProgress;
  }

  async onAuthDragEnd() {
    const orders: Array<ProxyAuthenticationDataOrderData> = [];
    this.proxyConfig.authentications.forEach((x, i) => {
      x.order = i;
      orders.push({ authId: x.id, order: x.order });
    });
    await this.proxyAuthService.UpdateAuthOrdersAsync(orders);
  }

  async regenChallenge() {
    if (!confirm(`Regenerate challenge? This will cause clients to have to complete it again.`)) return;
    await this.proxyAuthService.ResetChallengesForAuthenticationAsync({
      authenticationId: this.authInDialog.id,
      identityId: null,
    });
  }

  onConditionSave(cond: ConditionData) {
    const index = this.proxyConfig.proxyConditions.findIndex((x) => x.id == cond.id);
    if (index == -1) this.proxyConfig.proxyConditions.push(cond);
    else this.proxyConfig.proxyConditions[index] = cond;
  }

  onConditionDelete(cond: ConditionData) {
    const index = this.proxyConfig.proxyConditions.findIndex((x) => x.id == cond.id);
    if (index != -1) this.proxyConfig.proxyConditions.splice(index, 1);
  }

  onAuthConditionSave(cond: ConditionData) {
    const parent = this.proxyConfig.authentications.find((x) => x.id == cond.parentId);
    if (!parent) return;
    const index = parent.conditions.findIndex((x) => x.id == cond.id);
    if (index == -1) parent.conditions.push(cond);
    else parent.conditions[index] = cond;
  }

  onAuthConditionDelete(cond: ConditionData) {
    const parent = this.proxyConfig.authentications.find((x) => x.id == cond.parentId);
    if (parent != null) {
      const index = parent.conditions.findIndex((x) => x.id == cond.id);
      if (index != -1) parent.conditions.splice(index, 1);
    }
  }
}
</script>

<template>
  <div class="proxyconfig-page">
    <!-- Config not found -->
    <div v-if="notFound" class="feedback-message">Proxy config was not found.</div>

    <!-- Initial loading -->
    <loader-component :status="proxyConfigService.status" :value="allowInitialLoader" />

    <!-- Has config -->
    <div v-if="proxyConfig">
      <!-- Proxy config -->
      <div class="block">
        <div class="block-title">Proxy config</div>
        <proxy-config-editor v-model:value="proxyConfig" :disabled="isLoading" />

        <div class="mt-1">
          <button-component @click="saveConfig" :disabled="isLoading" class="ml-0 mr-1">Save</button-component>
          <button-component @click="deleteConfig" :disabled="isLoading" class="ml-0 danger">Delete</button-component>
          <loader-component
            :status="proxyConfigService.status"
            inline
            inlineYAdjustment="-4px"
            :allow="allowInitialLoader === false"
          />
        </div>
      </div>

      <!-- Conditions -->
      <div class="block mt-4">
        <div class="block-title">Conditions</div>
        <p v-if="proxyConfig.proxyConditions.length > 0">Proxy available when the conditions below are met.</p>
        <condition-datas-editor
          class="auth-conditions"
          :value="proxyConfig.proxyConditions"
          :disabled="isLoading"
          :parentId="proxyConfig.id"
          parentType="ProxyConfig"
          @save="onConditionSave"
          @delete="onConditionDelete"
          summaryLabel="Proxy is available:"
        />
      </div>

      <!-- Auths -->
      <div class="block mt-4 mb-5">
        <div class="block-title">Required authentications</div>
        <div v-if="proxyConfig.authentications.length == 0">
          No authentications challenges configured - the proxy is open for all.
        </div>
        <draggable
          v-if="proxyConfig"
          v-model="proxyConfig.authentications"
          item-key="id"
          handle=".handle"
          class="authentication"
          :disabled="isLoading"
          @end="onAuthDragEnd"
        >
          <template #item="{ element }">
            <div class="draggable-auth">
              <div class="auth-item-wrapper">
                <div class="handle">
                  <div class="material-icons icon">drag_handle</div>
                </div>
                <div class="auth-item" @click="showAuthDialog(element)">
                  <!-- <div class="material-icons icon">key</div> -->
                  <div>{{ createAuthSummary(element) }}</div>
                </div>
              </div>
              <div>
                <condition-datas-editor
                  class="auth-conditions"
                  :value="element.conditions"
                  :disabled="isLoading"
                  :parentId="element.id"
                  parentType="ProxyAuthenticationData"
                  @save="onAuthConditionSave"
                  @delete="onAuthConditionDelete"
                  summaryLabel="Authentication is required:"
                />
              </div>
            </div>
          </template>
        </draggable>
        <button-component @click="onAddAuthClicked" small secondary class="add-auth-button ml-0" icon="add"
          >Add authentication</button-component
        >
      </div>

      <!-- Auth Dialog -->
      <dialog-component v-model:value="authDialogVisible" max-width="800" persistent>
        <template #header>Proxy authentication</template>
        <template #footer>
          <button-component @click="saveAuth" :disabled="isLoading" class="primary ml-0">Save</button-component>
          <button-component @click="authDialogVisible = false" :disabled="isLoading" class="secondary"
            >Cancel</button-component
          >
          <button-component
            @click="regenChallenge"
            :disabled="isLoading"
            class="secondary"
            v-if="authInDialog?.id != emptyGuid"
            >Regenerate challenge</button-component
          >
          <button-component
            @click="deleteAuth"
            :disabled="isLoading"
            class="danger"
            v-if="authInDialog?.id != emptyGuid"
            >Delete</button-component
          >
          <loader-component :status="proxyAuthService.status" inline inlineYAdjustment="-4px" />
        </template>
        <proxy-authentication-data-editor
          v-if="authInDialog"
          :key="authInDialog.id"
          v-model:value="authInDialog"
          :disabled="isLoading"
        />
      </dialog-component>
    </div>
  </div>
</template>

<style scoped lang="scss">
.proxyconfig-page {
  padding-top: 20px;

  .auth-item-wrapper {
    display: flex;
    align-items: center;
    .handle {
      cursor: grab;
    }
  }

  .authentication {
    border-bottom: 2px solid var(--color--panel-light);
    padding-bottom: 8px;
  }

  .auth-item {
    display: inline-flex;
    align-items: center;
    cursor: pointer;
    padding: 10px 5px;

    &:hover {
      text-decoration: none;
      background-color: var(--color--hover-bg);
    }

    .icon {
      margin-right: 5px;
    }
  }
  .add-auth-button {
    margin-top: 20px !important;
  }

  .auth-conditions {
    padding: 10px 5px;
    margin-left: 20px;
  }
  .add-cond-button {
    margin-left: 30px !important;
  }
}
</style>
