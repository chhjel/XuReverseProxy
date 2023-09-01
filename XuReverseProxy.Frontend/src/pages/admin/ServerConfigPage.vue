<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ServerConfigService from "@services/ServerConfigService";
import { RuntimeServerConfigItem } from "@generated/Models/Core/RuntimeServerConfigItem";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		CheckboxComponent,
		CodeInputComponent
	}
})
export default class ServerConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ServerConfigService = new ServerConfigService();
	runtimeConfigs: Array<RuntimeServerConfigItem> = [];

	config_NotFoundHtml: string = '';
	config_ClientBlockedHtml: string = '';
	config_ClientBlockedResponseCode: string = '';

	async mounted() {
		await this.loadConfig();

		this.config_NotFoundHtml = this.getConfigString('NotFoundHtml');
		this.config_ClientBlockedHtml = this.getConfigString('ClientBlockedHtml');
		this.config_ClientBlockedResponseCode = this.getConfigString('ClientBlockedResponseCode');
	}

	async loadConfig() {
		const result = await this.service.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.runtimeConfigs = result.data || null;
	}

	get isLoading(): boolean { return this.service.status.inProgress; }

	getConfigBool(key: string) {
		const item = this.runtimeConfigs.find(x => x.key == key);
		return item?.value == "true";
	}

	async toggleConfig(key: string) {
		if (this.isLoading) return;

		const item = this.runtimeConfigs.find(x => x.key == key);
		if (item == null) return;

		const oldValue = item.value;
		const newValue = !JSON.parse(item.value);
		item.value = `${(newValue)}`;

		const result = await this.service.CreateOrUpdateAsync(item);
		if (!result.success) item.value = oldValue;
	}

	getConfigString(key: string) {
		const item = this.runtimeConfigs.find(x => x.key == key);
		return item?.value || "";
	}

	async saveConfig(key: string, value: string) {
		const item = this.runtimeConfigs.find(x => x.key == key);
		if (item == null) return;
		
		const oldValue = item.value;
		item.value = value;
		
		const result = await this.service.CreateOrUpdateAsync(item);
		if (!result.success) item.value = oldValue;
	}

	async saveClientBlockedSection() {
		await this.saveConfig('ClientBlockedHtml', this.config_ClientBlockedHtml);
		await this.saveConfig('ClientBlockedResponseCode', this.config_ClientBlockedResponseCode);
	}
}
</script>

<template>
	<div class="serverconfig-page">
		<div class="runtime-config">
			<div class="block mt-4">
				<checkbox-component 
					label="Proxy server enabled"
					offLabel="Proxy server disabled"
					:disabled="isLoading"
					:value="getConfigBool('EnableForwarding')"
					@click="toggleConfig('EnableForwarding')"
					class="mt-2 mb-2" />
					
				<checkbox-component 
					label="Require admin login for manual approval page"
					offLabel="Require admin login for manual approval page"
					:disabled="isLoading"
					:value="getConfigBool('EnableManualApprovalPageAuthentication')"
					@click="toggleConfig('EnableManualApprovalPageAuthentication')"
					class="mt-2 mb-2" />
			</div>

			<div class="block-title mt-4">404 HTML</div>
			<div class="block">
				<code-input-component v-model:value="config_NotFoundHtml" :disabled="isLoading" language="html" style="height: 400px" />
				<button-component @click="saveConfig('NotFoundHtml', config_NotFoundHtml)" class="ml-0 mt-3">Save</button-component>
			</div>

			<div class="block-title mt-4">Client blocked HTML</div>
			<div class="block">
				<code-input-component v-model:value="config_ClientBlockedHtml" :disabled="isLoading" language="html" style="height: 400px" />
        		<p><code>&#123;&#123;blocked_message&#125;&#125;</code> can be used as a placeholder for the blocked message.</p>
		    	<text-input-component label="Response code" v-model:value="config_ClientBlockedResponseCode" placeholder="401" class="blocked-response-code-input" />
				<button-component @click="saveClientBlockedSection" class="ml-0 mt-3">Save</button-component>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.serverconfig-page {
	.blocked-response-code-input {
		max-width: 200px;
	}
}
</style>
