<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
	}
})
export default class ProxyConfigsPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    proxyConfigService: ProxyConfigService = new ProxyConfigService();
	proxyConfigs: Array<ProxyConfig> = [];

	async mounted() {
		const result = await this.proxyConfigService.GetProxyConfigsAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.proxyConfigs = result.data || [];
	}
}
</script>

<template>
	<div class="proxyconfigs-page">
		<div v-for="config in proxyConfigs" :key="config.id">
			<router-link :to="{ name: 'proxyconfig', params: { configId: config.id }}">
				<code>{{ config }}</code>
			</router-link>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigs-page {

}
</style>
