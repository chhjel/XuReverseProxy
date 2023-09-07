<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeManualApprovalFrontendModel } from "@generated/Models/Core/ProxyChallengeTypeManualApprovalFrontendModel";
import ProxyAuthService from "@services/ProxyAuthService";
import { RequestApprovalResponseModel } from "@generated/Models/Core/RequestApprovalResponseModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeManualApprovalComponent extends Vue {
  	@Prop()
	options: ProxyChallengeTypeManualApprovalFrontendModel;
	
    service!: ProxyAuthService;
	result: RequestApprovalResponseModel | null = null;

	async mounted() {
    	this.service = new ProxyAuthService('ProxyChallengeTypeManualApproval', this.options.authenticationId);
	}

	get hasRequested(): boolean { return this.options.hasRequested || this.result?.success == true; }

	get isLoading(): boolean { return this.service?.status?.inProgress == true; }

	get formattedEasyCode(): string {
		if (!this.options.easyCode || this.options.easyCode.length < 6) return this.options.easyCode;
		else return `${this.options.easyCode.substring(0, 2)} ${this.options.easyCode.substring(2, 4)} ${this.options.easyCode.substring(4, 6)}`
	}

	async onRequestAccessClicked(): Promise<any> {
		await this.requestAccess();
	}

	async requestAccess(): Promise<any> {
		this.result = null;
		this.result = await this.service.RequestAsync('RequestApprovalAsync', {}) as RequestApprovalResponseModel;
	}

	get statusMessage(): string {
		if (this.isLoading) return "Requesting access..";
		else if (this.result?.error) return this.result.error;
		else if (this.hasRequested) return "Waiting for manual approval";
		else return '';
	}
}
</script>

<template>
	<div class="challenge-manual-approval">
		<div class="challenge-header">
			<div class="material-icons icon">key</div>
			<div class="challenge-title">Manual approval required</div>
		</div>

		<div class="easycode-wrapper" v-if="hasRequested">
			<div>Reference code</div>
			<div class="easycode">{{ formattedEasyCode }}</div>
		</div>

		<div v-if="!hasRequested" class="challenge-manual-approval__inputs">
			<button-component @click="onRequestAccessClicked" :disabled="isLoading" class="ml-0 secondary">Request access</button-component>
		</div>
		
		<div v-if="statusMessage" class="challenge-manual-approval__status" :class="{ 'error': result?.success == false }">
			{{ statusMessage }}
		</div>
	</div>
</template>

<style scoped lang="scss">
.challenge-manual-approval {
	.easycode-wrapper {
		margin-top: 15px;
		padding: 20px;
		text-align: center;
		background-color: #121212;
		font-size: 22px;

		.easycode {
			font-size: calc(min(max(40px, 11vw),70px));
		}
	}

	&__inputs {
		max-width: 300px;
    	margin: 20px auto 0 auto;
	}

	&__status {
		margin-top: 15px;
		text-align: center;
		font-size: 15px;
		color: var(--color--text-dark);
		&.error {
			color: var(--color--danger-lighten);
		}
	}
	
	.meta {
		font-size: 14px;
	}

	.button {
		width: 100%;
    	height: 50px;
		padding: 0;
	}
}
</style>
