<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import ServerConfigService from "@services/ServerConfigService";
import { RuntimeServerConfigItem } from "@generated/Models/Core/RuntimeServerConfigItem";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
	}
})
export default class ServerConfigPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    service: ServerConfigService = new ServerConfigService();
	runtimeConfigs: Array<RuntimeServerConfigItem> = [];

	async mounted() {
		await this.loadConfig();
	}

	async loadConfig() {
		const result = await this.service.GetAllAsync();
		if (!result.success) {
			console.error(result.message);
		}
		this.runtimeConfigs = result.data || null;
	}

	get isLoading(): boolean { return this.service.status.inProgress; }
}
</script>

<template>
	<div class="serverconfig-page">
		<code>{{ runtimeConfigs }}</code>
	</div>
</template>

<style scoped lang="scss">
.serverconfig-page {

}
</style>
