<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu
	}
})
export default class AdminOverviewPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
    // loginService: LoginService = new LoginService();

	async mounted() {
	}
}
</script>

<template>
	<div class="overview-page">
		<div>
			<h4>Page: Overview (default)</h4>
			<p>Some status here? List proxies? Some quick metrics? Last used, num clients etc?</p>
		</div>

		<div>
			<h4>Page: Proxies</h4>
			<p>CRUD for proxy config</p>
		</div>

		<div>
			<h4>Page: Config</h4>
			<p>Show static, non-secret config & runtime config</p>
		</div>

		<div>
			<h4>Page: Clients</h4>
			<p>Show static, non-secret config & runtime config</p>
		</div>

		<div>
			<h4>Page: Jobs</h4>
			<p>Show jobs status. Simple list w/ name of job, description, status icon & text, and last result.</p>
		</div>
	</div>
</template>

<style scoped lang="scss">
.overview-page {

}
</style>
