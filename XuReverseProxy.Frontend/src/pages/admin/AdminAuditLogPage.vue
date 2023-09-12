<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import AdminAuditLogComponent from "@components/admin/AdminAuditLogComponent.vue";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent,
		AdminAuditLogComponent
	}
})
export default class AdminAuditLogPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
	async mounted() {
	}
}
</script>

<template>
	<div class="adminauditlog-page">
		<Admin-audit-log-component />
	</div>
</template>

<style scoped lang="scss">
.adminauditlog-page {
	padding-top: 20px;
}
</style>
