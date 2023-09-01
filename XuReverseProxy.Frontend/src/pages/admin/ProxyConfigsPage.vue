<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyConfigService from "@services/ProxyConfigService";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";
import { EmptyGuid } from "@utils/Constants";

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
		const result = await this.proxyConfigService.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.proxyConfigs = result.data || [];
	}

	async addNewProxyConfig() {
		let config: ProxyConfig = {
			id: EmptyGuid,
			enabled: false,
			authentications: [],
			name: 'New Proxy Config',
			challengeDescription: '',
			port: null,
			subdomain: '',
			challengeTitle: '',
			destinationPrefix: 'http://192.168.2.',
			showCompletedChallenges: true,
			showChallengesWithUnmetRequirements: true
		};
		const result = await this.proxyConfigService.CreateOrUpdateAsync(config);
		if (!result.success) {
			console.error(result.message);
			alert(result.message);
		} else {
			config = result.data;
			this.proxyConfigs.push(config);
			this.$router.push({ name: 'proxyconfig', params: { configId: config.id } });
		}
	}

	get sortedConfigs(): Array<ProxyConfig> {
		return this.proxyConfigs.sort((a,b) => a.name?.localeCompare(b.name));
	}
}
</script>

<template>
	<div class="proxyconfigs-page">
		<div v-for="config in sortedConfigs" :key="config.id">
			<router-link :to="{ name: 'proxyconfig', params: { configId: config.id }}">
				<code>{{ config }}</code>
			</router-link>
		</div>
		<button-component @click="addNewProxyConfig" class="primary ml-0">Add new config</button-component>
	</div>
</template>

<style scoped lang="scss">
.proxyconfigs-page {

}
</style>
