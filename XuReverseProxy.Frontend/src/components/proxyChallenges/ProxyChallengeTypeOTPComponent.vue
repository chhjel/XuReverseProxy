<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeOTPFrontendModel } from "@generated/Models/Core/ProxyChallengeTypeOTPFrontendModel";
import ProxyAuthService from "@services/ProxyAuthService";
import { TrySendOTPResponseModel } from "@generated/Models/Core/TrySendOTPResponseModel";
import { TrySolveOTPResponseModel } from "@generated/Models/Core/TrySolveOTPResponseModel";
import { TrySolveOTPRequestModel } from "@generated/Models/Core/TrySolveOTPRequestModel";
import { LoadStatus } from "@services/ServiceBase";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeOTPComponent extends Vue {
  	@Prop()
	options: ProxyChallengeTypeOTPFrontendModel;
	
	otp: string = '';
	hasSentCode: boolean = false;
	
    service: ProxyAuthService = new ProxyAuthService('ProxyChallengeTypeOTP');
	statusMessage: string = '';
	statusIsError: boolean = false;

	async mounted() {
		this.hasSentCode = this.options.hasSentCode;
		if (this.options.codeSentAt) {
			this.setStatusMessage(`Code last sent at ${this.formatDate(this.options.codeSentAt)}`);
		}
	}

	get isLoading(): boolean { return this.service.status.inProgress; }

	async onSendCodeClicked(): Promise<any> {
		await this.sendOtp();
	}

	async onValidateCodeClicked(): Promise<any> {
		await this.validateOtp();
	}

	async sendOtp(): Promise<any> {
		this.setStatusMessage('Sending code..');
		const result = await this.service.RequestAsync('TrySendOTPAsync', {}) as TrySendOTPResponseModel;
		if (result.success) {
			this.hasSentCode = true;
			this.setStatusMessage('Code sent');
		} else {
			this.setStatusMessage(result.error, true);
		}
	}

	async validateOtp(): Promise<any> {
		this.setStatusMessage('Validating..');
		const payload: TrySolveOTPRequestModel = {
			code: this.otp
		};
		const result = await this.service.RequestAsync('TrySolveOTPAsync', payload) as TrySolveOTPResponseModel;
		if (result.success) {
			this.hasSentCode = true;
			this.setStatusMessage('Code validated');
			setTimeout(() => { this.$emit('solved'); }, 1000);
		} else {
			this.setStatusMessage(result.error, true);
		}
	}

	setStatusMessage(text: string, isError: boolean = false): void {
		this.statusMessage = text;
		this.statusIsError = isError === true;
	}

	formatDate(raw: Date | string): string {
		if (raw == null) return '';
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date.toLocaleString();
	}
}
</script>

<template>
	<div class="challenge-otp">
		<div class="challenge-header">
			<div class="material-icons icon">key</div>
			<div class="challenge-title">OTP verification</div>
		</div>

		<div v-if="options.description" class="challenge-otp__description">
			<p>{{ options.description }}</p>
		</div>

		<div class="challenge-otp__inputs">

			<div v-if="!hasSentCode">
				<button-component @click="onSendCodeClicked" :disabled="isLoading" class="ml-0 secondary">Send code</button-component>
			</div>

			<div v-if="hasSentCode">
				<text-input-component
					v-model:value="otp"
					placeholder="One-time code"
					:disabled="isLoading"
					@keydown.enter="onValidateCodeClicked"
					/>
				<button-component @click="onValidateCodeClicked" :disabled="isLoading" class="ml-0 secondary">Validate code</button-component>
				<button-component @click="onSendCodeClicked" :disabled="isLoading" class="ml-0 secondary">Re-send code</button-component>
			</div>
		</div>

		<div v-if="statusMessage" class="challenge-otp__status" :class="{ 'error': statusIsError }">
			{{ statusMessage }}
		</div>
	</div>
</template>

<style scoped lang="scss">
.challenge-otp {
	&__description {
		color: var(--color--text-dark);
	}

	&__inputs {
		max-width: 300px;
    	margin: 20px auto 0 auto;
		/* text-align: center; */
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
		margin-top: 15px;
		text-align: center;
		font-size: 15px;
		color: var(--color--text-dark);
	}

	.button {
		width: 100%;
    	height: 50px;
		padding: 0;
		margin-bottom: 0;
	}
}
</style>
