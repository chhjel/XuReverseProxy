<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Provide } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import EventBus from "@utils/EventBus";

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

	async mounted() {
        document.addEventListener('keyup', this.onDocumentKeyDownOrDown);
        document.addEventListener('keydown', this.onDocumentKeyDownOrDown);
	}

    beforeUnmount(): void {
        document.removeEventListener('keyup', this.onDocumentKeyDownOrDown);
        document.removeEventListener('keydown', this.onDocumentKeyDownOrDown);
    }
	
    onDocumentKeyDownOrDown(e: KeyboardEvent): void {
        if (e.key == 'Escape') {
            EventBus.notify("onEscapeClicked", e);
        }
    }
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
    max-width: 800px;
    margin: auto;
	padding: 40px;
	@media (max-width: 800px) {
		padding: 10px;
		margin-top: 20px;
	}
}
</style>
