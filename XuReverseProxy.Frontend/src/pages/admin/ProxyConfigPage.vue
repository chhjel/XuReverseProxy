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

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		DialogComponent
	}
})
export default class ProxyConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
    proxyAuthService: ProxyAuthenticationDataService = new ProxyAuthenticationDataService();
    proxyAuthConditionService: ProxyAuthenticationConditionService = new ProxyAuthenticationConditionService();

	proxyConfig: ProxyConfig | null = null;
	proxyConfigJson: string = '';
	notFound: boolean = false;
	authDialogVisible: boolean = false;
	authInDialog: ProxyAuthenticationData | null = null;

	async mounted() {
		await this.loadConfig();

		const auth = this.proxyConfig?.authentications[0];
		if (auth != null) this.showAuthDialog(auth);
	}

	async loadConfig() {
		const configId = StringUtils.firstOrDefault(this.$route.params.configId);
		const result = await this.proxyConfigService.GetAsync(configId);
		if (!result.success) {
			console.error(result.message);
		}
		this.proxyConfig = result.data || null;

		if (result.data) this.proxyConfigJson = JSON.stringify(result.data, null, "  ");
	}

	// Note: only saves config itself, not auths etc
	async saveConfig() {
		const config = JSON.parse(this.proxyConfigJson);
		const result = await this.proxyConfigService.CreateOrUpdateAsync(config);
		if (!result.success) {
			console.error(result.message);
		} else {
			this.proxyConfig = result.data;
			this.proxyConfigJson = JSON.stringify(result.data, null, "  ");
		}
	}

	async deleteConfig() {
		console.log("todo");
	}

	showAuthDialog(auth: ProxyAuthenticationData) {
		this.authInDialog = auth;
		this.authDialogVisible = true;
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
			<div class="mt-4 block">
				<div class="block-title">Proxy config</div>
				<div class="input-wrapper">
					<textarea v-model="proxyConfigJson" rows="10" style="width: 100%;"></textarea><br>
				</div>
				<button-component @click="saveConfig" :disabled="isLoading">Save</button-component>
				<button-component @click="deleteConfig" :disabled="isLoading" class="danger">Delete</button-component>
			</div>

			<div class="mt-4 block">
				<div class="block-title">Proxy authentications</div>
				<div v-for="auth in sortedAuths" :key="auth.id" @click="showAuthDialog(auth)">
					<div>{{ auth.challengeTypeId }}</div>
					<div v-for="cond in auth.conditions" :key="cond.id">
						<div> * Condition: {{ cond.conditionType }}</div>
					</div>
				</div>
			</div>

			<dialog-component v-model:value="authDialogVisible" max-width="600" persistent>
				<template #header>Proxy authentication</template>
				<template #footer>
					<button-component class="primary ml-0" @click="authDialogVisible = false">Save</button-component>
					<button-component class="secondary" @click="authDialogVisible = false">Cancel</button-component>
				</template>
				<code>{{ authInDialog }}</code>
			</dialog-component>

			<hr class="mt-5 mb-5">
			<code>{{ proxyConfig }}</code>
			<hr>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigs-page {

}
</style>
