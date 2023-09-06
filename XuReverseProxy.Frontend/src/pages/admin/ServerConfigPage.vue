<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject, Ref } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ServerConfigService from "@services/ServerConfigService";
import { RuntimeServerConfigItem } from "@generated/Models/Core/RuntimeServerConfigItem";
import CheckboxComponent from "@components/inputs/CheckboxComponent.vue";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import { ClientBlockedHtmlPlaceholders, PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		CheckboxComponent,
		CodeInputComponent,
		ExpandableComponent,
		PlaceholderInfoComponent
	}
})
export default class ServerConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ServerConfigService = new ServerConfigService();
	runtimeConfigs: Array<RuntimeServerConfigItem> = [];

    @Ref() readonly clientBlockedHtmlEditor!: any;

	clientBlockedHtmlPlaceholdersExtra: Array<PlaceholderInfo> = [{
		name: "blocked_message",
		description: "Can be used as a placeholder for the blocked message."
	}];
	clientBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = ClientBlockedHtmlPlaceholders;
	config_NotFoundHtml: string = '';
	config_ClientBlockedHtml: string = '';
	config_ClientBlockedResponseCode: string = '';
	config_IPBlockedHtml: string = '';
	config_IPBlockedResponseCode: string = '';

	async mounted() {
		await this.loadConfig();

		this.config_NotFoundHtml = this.getConfigString('NotFoundHtml');
		this.config_ClientBlockedHtml = this.getConfigString('ClientBlockedHtml');
		this.config_ClientBlockedResponseCode = this.getConfigString('ClientBlockedResponseCode');
		this.config_IPBlockedHtml = this.getConfigString('IPBlockedHtml');
		this.config_IPBlockedResponseCode = this.getConfigString('IPBlockedResponseCode');
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

	async saveIPBlockedSection() {
		await this.saveConfig('IPBlockedHtml', this.config_IPBlockedHtml);
		await this.saveConfig('IPBlockedResponseCode', this.config_IPBlockedResponseCode);
	}

	insertPlaceholderClientBlockedHtml(val: string): void {
		this.clientBlockedHtmlEditor.insertText(val);
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
				<code-input-component v-model:value="config_NotFoundHtml" :disabled="isLoading" language="html" height="400px"/>
				<button-component @click="saveConfig('NotFoundHtml', config_NotFoundHtml)" class="ml-0 mt-3">Save</button-component>
			</div>

			<div class="block-title mt-4">Client blocked HTML</div>
			<div class="block">
				<code-input-component v-model:value="config_ClientBlockedHtml" :disabled="isLoading"
					language="html" height="400px" ref="clientBlockedHtmlEditor" />
				<expandable-component header="Supported placeholders">
					<placeholder-info-component
						:placeholders="clientBlockedHtmlPlaceholders"
						:additionalPlaceholders="clientBlockedHtmlPlaceholdersExtra" 
						@insertPlaceholder="insertPlaceholderClientBlockedHtml"
						/>
				</expandable-component>
		    	<text-input-component label="Response code" v-model:value="config_ClientBlockedResponseCode" placeholder="401" class="blocked-response-code-input" />
				<button-component @click="saveClientBlockedSection" class="ml-0 mt-3">Save</button-component>
			</div>

			<div class="block-title mt-4">IP blocked HTML</div>
			<div class="block">
				<code-input-component v-model:value="config_IPBlockedHtml" :disabled="isLoading" language="html" height="400px" />
		    	<text-input-component label="Response code" v-model:value="config_IPBlockedResponseCode" placeholder="401" class="blocked-response-code-input" />
				<button-component @click="saveIPBlockedSection" class="ml-0 mt-3">Save</button-component>
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
