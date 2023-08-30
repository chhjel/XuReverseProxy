<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Provide } from 'vue-property-decorator'
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
export default class DashboardPage extends Vue {
  	@Prop()
	@Provide()
	options: AdminPageFrontendModel;
	
    // loginService: LoginService = new LoginService();

	async mounted() {

	}

	// get isLoading(): boolean { return this.loginService.status.inProgress; }
}
</script>

<template>
	<div class="admin-app">
		<admin-nav-menu />

  		<router-view :options="options"></router-view>
	</div>
</template>

<style scoped lang="scss">
.admin-app {

}
</style>
