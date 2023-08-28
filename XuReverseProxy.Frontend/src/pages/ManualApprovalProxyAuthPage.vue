<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ManualApprovalProxyAuthPageFrontendModel } from "@generated/Models/Web/ManualApprovalProxyAuthPageFrontendModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ManualApprovalProxyAuthPage extends Vue {
  	@Prop()
	options: ManualApprovalProxyAuthPageFrontendModel;
	
    // loginService: LoginService = new LoginService();

	async mounted() {
	}

	get isLoading(): boolean { return false; }
	// get isLoading(): boolean { return this.loginService.status.inProgress; }
	
	async onApproveClicked(): Promise<any> { }
	async onUnApproveClicked(): Promise<any> { }
	async onBlockClicked(): Promise<any> { }

	// /proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/approve
	// returns bool

	// /proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/unapprove
	// returns bool, but no need to check it

	// /proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/block
	// SetClientBlockedFromManualApprovalRequestMessage
	// returns bool
}
</script>

<template>
	<div class="manual-approval-page">
		
		<button-component @click="onApproveClicked" :disabled="isLoading" class="ml-0">Approve</button-component>
		<button-component @click="onUnApproveClicked" :disabled="isLoading" class="ml-0">Remove approval</button-component>
		<button-component @click="onBlockClicked" :disabled="isLoading" class="ml-0">Block client</button-component>

		<hr class="mt-4 mb-4">
		<code>{{ options }}</code>
	</div>
</template>

<style scoped lang="scss">
.manual-approval-page {

}
</style>
