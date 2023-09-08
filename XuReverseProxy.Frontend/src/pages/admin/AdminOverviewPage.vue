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
		// todo: show some usefull details on overview page. For now redirect to configs
		this.$router.push({ name: 'proxyconfigs' });
	}
}
</script>

<template>
	<div class="overview-page">
		todo
	</div>
</template>

<style scoped lang="scss">
.overview-page {

}
</style>
