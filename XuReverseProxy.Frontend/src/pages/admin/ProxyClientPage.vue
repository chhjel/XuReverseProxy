<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ProxyClientIdentityService from "@services/ProxyClientIdentityService";
import { ProxyClientIdentity } from "@generated/Models/Core/ProxyClientIdentity";
import StringUtils from "@utils/StringUtils";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
	}
})
export default class ProxyClientPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ProxyClientIdentityService = new ProxyClientIdentityService();
	client: ProxyClientIdentity | null = null;
	clientId: string = '';

	async mounted() {
		this.clientId = StringUtils.firstOrDefault(this.$route.params.clientId);
		const result = await this.service.GetAsync(this.clientId);
		if (!result.success) {
			console.error(result.message);
		} else {
			this.client = result.data;
		}
	}
}
</script>

<template>
	<div class="proxyclient-page">
		<div v-if="client">
			<code>{{ client }}</code>
		</div>
	</div>
</template>

<style scoped lang="scss">
.proxyclient-page {

}
</style>
