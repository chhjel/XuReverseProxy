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
import { createProxyAuthenticationSummary } from "@utils/ProxyAuthenticationDataUtils";
import { createProxyAuthenticationConditionSummary } from "@utils/ProxyAuthenticationConditionUtils";
import IdUtils from "@utils/IdUtils";
import { EmptyGuid, ProxyAuthChallengeTypeOptions, ProxyAuthConditionTypeOptions } from "@utils/Constants";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		CheckboxComponent,
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
	conditionDialogVisible: boolean = false;
	conditionInDialog: ProxyAuthenticationCondition | null = null;
	emptyGuid: string = EmptyGuid;

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
		if (!confirm('Delete this proxy config?')) return;

		const result = await this.proxyConfigService.DeleteAsync(this.proxyConfig.id);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			this.$router.push({ name: 'proxyconfigs' });
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
			const index = this.proxyConfig.authentications.findIndex(x => x.id == this.authInDialog.id);
			if (index != -1) this.proxyConfig.authentications.splice(index, 1);
			this.authDialogVisible = false;
			this.authInDialog = null;
		}
	}
	
	async saveCondtion() {
		const result = await this.proxyAuthConditionService.CreateOrUpdateAsync(this.conditionInDialog);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			this.conditionInDialog = result.data;
			this.conditionDialogVisible = false;
			this.onConditionSaved(result.data);
		}
	}

	async deleteCondition() {
		const result = await this.proxyAuthConditionService.DeleteAsync(this.conditionInDialog.id);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			const auth = this.proxyConfig.authentications.find(x => x.id == this.conditionInDialog.authenticationDataId);
			if (auth != null) {
				const index = auth.conditions.findIndex(x => x.id == this.conditionInDialog.id);
				if (index != -1) auth.conditions.splice(index, 1);
			}
			this.conditionDialogVisible = false;
			this.conditionInDialog = null;
		}
	}
	
	showAuthDialog(auth: ProxyAuthenticationData) {
		this.authInDialog = auth;
		this.authDialogVisible = true;
	}

	onAuthSaved(auth: ProxyAuthenticationData) {
		const index = this.proxyConfig.authentications.findIndex(x => x.id == auth.id);
		if (index == -1) 
			this.proxyConfig.authentications.push(auth);
		else
			this.proxyConfig.authentications[index] = auth;
	}

	showConditionDialog(cond: ProxyAuthenticationCondition) {
		this.conditionInDialog = cond;
		this.conditionDialogVisible = true;
	}

	onConditionSaved(cond: ProxyAuthenticationCondition) {
		const auth = this.proxyConfig.authentications.find(x => x.id == cond.authenticationDataId);
		if (!auth) return;
		const index = auth.conditions.findIndex(x => x.id == cond.id);
		if (index == -1) 
			auth.conditions.push(cond);
		else
			auth.conditions[index] = cond;
	}

	createAuthSummary(auth: ProxyAuthenticationData): string {
		return createProxyAuthenticationSummary(auth);
	}

	createAuthCondSummary(cond: ProxyAuthenticationCondition): string {
		return createProxyAuthenticationConditionSummary(cond);
	}

	async onAddAuthClicked(): Promise<any> {
		const auth: ProxyAuthenticationData = {
			id: EmptyGuid,
			challengeTypeId: ProxyAuthChallengeTypeOptions[0].typeId,
			challengeJson: '{}',
			proxyConfig: null,
			proxyConfigId: this.proxyConfig.id,
			solvedDuration: null,
			solvedId: IdUtils.generateId(),
			order: 0,
			conditions: [],
		};
		this.showAuthDialog(auth);
	}

	async onAddAuthConditionClicked(auth: ProxyAuthenticationData): Promise<any> {
		const cond: ProxyAuthenticationCondition = {
			id: EmptyGuid,
			authenticationDataId: auth.id,
			authenticationData: null,
			conditionType: ProxyAuthConditionTypeOptions[0].value,
			dateTimeUtc1: null,
			dateTimeUtc2: null,
			daysOfWeekUtc: null,
			timeOnlyUtc1: null,
			timeOnlyUtc2: null
		};
		this.showConditionDialog(cond);
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
			<div class="block mt-4">
				<div class="block-title">Proxy config</div>
				<proxy-config-editor
					v-model:value="proxyConfig"
					:disabled="isLoading" />

				<div class="mt-1">
					<button-component @click="saveConfig" :disabled="isLoading" class="ml-0">Save</button-component>
					<button-component @click="deleteConfig" :disabled="isLoading" class="danger">Delete</button-component>
				</div>
			</div>

			<!-- Auths -->
			<div class="block mt-4 mb-5">
				<div class="block-title">Required authorizations</div>
				<div v-if="sortedAuths.length == 0">No authorization challenges configuredm the proxy is open for all.</div>
				<div v-for="auth in sortedAuths" :key="auth.id" class="block">
					<div @click="showAuthDialog(auth)">{{ createAuthSummary(auth) }}</div>
					<div v-for="cond in auth.conditions">
						<div @click="showConditionDialog(cond)"> * Condition: {{ createAuthCondSummary(cond) }}</div>
					</div>
					<div @click="onAddAuthConditionClicked(auth)"> * [add condition]</div>
				</div>
				<div @click="onAddAuthClicked">[add authentication]</div>
				(//todo: dragdrop order)
			</div>

			<!-- Auth Dialog -->
			<dialog-component v-model:value="authDialogVisible" max-width="600" persistent>
				<template #header>Proxy authentication</template>
				<template #footer>
					<button-component @click="saveAuth" class="primary ml-0">Save</button-component>
					<button-component @click="authDialogVisible = false" class="secondary">Cancel</button-component>
					<button-component @click="deleteAuth" :disabled="isLoading" class="danger"
						v-if="authInDialog?.id != emptyGuid"
						>Delete</button-component>
				</template>
				<proxy-authentication-data-editor
					v-if="authInDialog"
					:key="authInDialog.id"
					v-model:value="authInDialog"
					:disabled="isLoading" />
			</dialog-component>

			<!-- Condition Dialog -->
			<dialog-component v-model:value="conditionDialogVisible" max-width="600" persistent>
				<template #header>Proxy authentication condition</template>
				<template #footer>
					<button-component @click="saveCondtion" class="primary ml-0">Save</button-component>
					<button-component @click="conditionDialogVisible = false" class="secondary">Cancel</button-component>
					<button-component @click="deleteCondition" :disabled="isLoading" class="danger"
						v-if="conditionInDialog?.id != emptyGuid"
						>Delete</button-component>
				</template>
				<proxy-authentication-condition-editor
					v-if="conditionInDialog"
					:key="conditionInDialog.id"
					v-model:value="conditionInDialog"
					:disabled="isLoading" />
			</dialog-component>
		</div>
	</div>
</template>

<style scoped lang="scss">
/* .proxyconfig-page {
} */
</style>
