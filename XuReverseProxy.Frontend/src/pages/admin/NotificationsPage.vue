<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import NotificationRuleService from "@services/NotificationRuleService";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent,
		AdminNavMenu,
		LoaderComponent
	}
})
export default class NotificationsPage extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
	service: NotificationRuleService = new NotificationRuleService();

	async mounted() {
	}
}
</script>

<template>
	<div class="notifications-page">
		<code>Todo: notifications edit</code>
		<loader-component :status="service.status" />
		<div v-if="service.status.hasDoneAtLeastOnce">
		</div>
	</div>
</template>

<style scoped lang="scss">
.notifications-page {
	padding-top: 20px;
}
</style>
