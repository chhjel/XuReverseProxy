<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import LoaderComponent from "@components/common/LoaderComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent
	}
})
export default class ProxyClientsPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ProxyClientIdentityService = new ProxyClientIdentityService();
	clients: Array<ProxyClientIdentity> = [];

	async mounted() {
		const result = await this.service.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.clients = result.data || [];
	}

	onClientClicked(client: ProxyClientIdentity): void {
		this.$router.push({ name: 'client', params: { clientId: client.id } });
	}

	get pagedClients(): Array<ProxyClientIdentity> {
		return this.clients;
	}
}
</script>

<template>
	<div class="proxyclients-page">
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
			<ul>
				Todo
				<li>Get paged</li>
				<li>Delete single/all/not used in a month/week/year</li>
			</ul>
			<div v-for="client in pagedClients">
				<router-link :to="`/client/${client.id}`">
					<code>{{ client }}</code>
				</router-link>
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyclients-page {
	padding-top: 20px;

}
</style>
