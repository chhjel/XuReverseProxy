<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { DashboardPageFrontendModel } from "@generated/Models/Web/DashboardPageFrontendModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class DashboardPage extends Vue {
  	@Prop()
	options: DashboardPageFrontendModel;
	
    // loginService: LoginService = new LoginService();

	async mounted() {
	}

	// get isLoading(): boolean { return this.loginService.status.inProgress; }
}
</script>

<template>
	<div class="dashboard-page">
		<a href="/auth/logout">Logout</a>
		
		// ToDo: dashboard
	</div>
</template>

<style scoped lang="scss">
.dashboard-page {

}
</style>
