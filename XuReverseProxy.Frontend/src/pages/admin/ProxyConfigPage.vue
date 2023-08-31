<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import ProxyConfigService from "@services/ProxyConfigService";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import StringUtils from "@utils/StringUtils";
import ProxyAuthenticationDataService from "@services/ProxyAuthenticationDataService";
import ProxyAuthenticationConditionService from "@services/ProxyAuthenticationConditionService";
import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import DialogComponent from "@components/common/DialogComponent.vue";
import ProxyConfigEditor from "@components/admin/proxyConfig/ProxyConfigEditor.vue";
import ProxyAuthenticationDataEditor from "@components/admin/proxyConfig/ProxyAuthenticationDataEditor.vue";
import ProxyAuthenticationConditionEditor from "@components/admin/proxyConfig/ProxyAuthenticationConditionEditor.vue";
import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		DialogComponent,
		ProxyConfigEditor,
		ProxyAuthenticationDataEditor,
		ProxyAuthenticationConditionEditor
	}
})
export default class ProxyConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
    proxyAuthService: ProxyAuthenticationDataService = new ProxyAuthenticationDataService();
    proxyAuthConditionService: ProxyAuthenticationConditionService = new ProxyAuthenticationConditionService();

	proxyConfig: ProxyConfig | null = null;
	notFound: boolean = false;
	authDialogVisible: boolean = false;
	authInDialog: ProxyAuthenticationData | null = null;

	async mounted() {
		await this.loadConfig();

		// const auth = this.proxyConfig?.authentications[2];
		// if (auth != null) this.showAuthDialog(auth);
	}

	async loadConfig() {
		const configId = StringUtils.firstOrDefault(this.$route.params.configId);
		const result = await this.proxyConfigService.GetAsync(configId);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		}
		this.proxyConfig = result.data || null;
	}

	// Note: only saves config itself, not auths etc
	async saveConfig() {
		const result = await this.proxyConfigService.CreateOrUpdateAsync(this.proxyConfig);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			this.proxyConfig = result.data;
		}
	}

	async deleteConfig() {
		alert("todo");
	}
	
	async saveAuth() {
		const result = await this.proxyAuthService.CreateOrUpdateAsync(this.authInDialog);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			this.authInDialog = result.data;
			this.authDialogVisible = false;
		}
	}

	async deleteAuth() {
		alert("todo");
	}

	showAuthDialog(auth: ProxyAuthenticationData) {
		this.authInDialog = auth;
		this.authDialogVisible = true;
	}

	onConditionChanged(auth: ProxyAuthenticationData, cond: ProxyAuthenticationCondition) {
		const index = auth.conditions.findIndex(x => x.id == cond.id);
		if (index == -1) return;
		auth.conditions[index] = cond;
	}

	get isLoading(): boolean { return this.proxyConfigService.status.inProgress; }

	get sortedAuths(): Array<ProxyAuthenticationData> {
		return this.proxyConfig.authentications.sort(({order:a}, {order:b}) => a-b);
	}
}
</script>

<template>
	<div class="proxyconfig-page">
		<!-- Config not found -->
		<div v-if="notFound" class="feedback-message">Proxy config was not found.</div>

		<!-- Initial loading -->
		<div v-if="isLoading && proxyConfig == null">Loading.. // todo, loader component</div>

		<!-- Has config -->
		<div v-if="proxyConfig">
			<!-- Proxy config -->
			<div class="mt-4 block">
				<div class="block-title">Proxy config</div>
				<div class="input-wrapper">
					<proxy-config-editor
						v-model:value="proxyConfig"
						:disabled="isLoading" />
				</div>
				<button-component @click="saveConfig" :disabled="isLoading">Save</button-component>
				<button-component @click="deleteConfig" :disabled="isLoading" class="danger">Delete</button-component>
			</div>

			<!-- Auths -->
			<div class="mt-4 block">
				<div class="block-title">Proxy authentications</div>
				<div v-for="auth in sortedAuths" :key="auth.id" @click="showAuthDialog(auth)">
					<div>{{ auth.challengeTypeId }}</div>
					<div v-for="cond in auth.conditions">
						<div> * Condition: {{ cond.conditionType }}</div>
						<proxy-authentication-condition-editor
							:value="cond"
							:disabled="isLoading"
							@update:value="e => onConditionChanged(auth, e)" />
					</div>
				</div>
				(//todo: dragdrop order)
			</div>

			<!-- Dialogs -->
			<dialog-component v-model:value="authDialogVisible" max-width="600" persistent>
				<template #header>Proxy authentication</template>
				<template #footer>
					<button-component @click="saveAuth" class="primary ml-0">Save</button-component>
					<button-component @click="authDialogVisible = false" class="secondary">Cancel</button-component>
					<button-component @click="deleteAuth" :disabled="isLoading" class="danger">Delete</button-component>
				</template>
				<proxy-authentication-data-editor
					v-if="authInDialog"
					:key="authInDialog.id"
					v-model:value="authInDialog"
					:disabled="isLoading" />
			</dialog-component>

			<hr class="mt-5 mb-5">
			<code>{{ proxyConfig }}</code>
			<hr>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfig-page {

}
</style>
