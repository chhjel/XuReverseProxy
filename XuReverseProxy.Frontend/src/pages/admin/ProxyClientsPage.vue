<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
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
}
</script>

<template>
	<div class="proxyclients-page">
		// Todo:
		<ul>
			<li>Get paged</li>
			<li>Delete single/all/not used in a month/week/year</li>
		</ul>
		<code>{{ clients }}</code>
	</div>
</template>

<style scoped lang="scss">
.proxyclients-page {

}
</style>
