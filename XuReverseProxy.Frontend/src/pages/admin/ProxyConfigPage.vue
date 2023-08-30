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

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
	}
})
export default class ProxyConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
	proxyConfig: ProxyConfig | null = null;
	notFound: boolean = false;

	async mounted() {
		await this.loadConfig();
	}

	async loadConfig() {
		const configId = StringUtils.firstOrDefault(this.$route.params.configId);
		const result = await this.proxyConfigService.GetProxyConfigAsync(configId);
		if (!result.success) {
			console.error(result.message);
		}
		this.proxyConfig = result.data || null;
	}

	get isLoading(): boolean { return this.proxyConfigService.status.inProgress; }
}
</script>

<template>
	<div class="proxyconfig-page">
		<code>{{ proxyConfig }}</code>

		<div v-if="notFound" class="feedback-message">Proxy config was not found.</div>
		<div v-if="isLoading && proxyConfig == null">Loading.. // todo, loader component</div>
		<div v-if="proxyConfig">

		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigs-page {

}
</style>
