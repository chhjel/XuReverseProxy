<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeLoginFrontendModel } from "@generated/Models/Core/ProxyChallengeTypeLoginFrontendModel";
import { VerifyLoginRequestModel } from "@generated/Models/Core/VerifyLoginRequestModel";
import ProxyAuthService from "@services/ProxyAuthService";
import { VerifyLoginResponseModel } from "@generated/Models/Core/VerifyLoginResponseModel";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeLoginComponent extends Vue {
  	@Prop()
	options: ProxyChallengeTypeLoginFrontendModel;
	
    service: ProxyAuthService = new ProxyAuthService('ProxyChallengeTypeLogin', '');
	username: string = '';
	password: string = '';
	totp: string = '';
	loginResult: boolean | null = null;
	
	statusMessage: string = '';
	statusIsError: boolean = false;

	async mounted() {
    	this.service = new ProxyAuthService('ProxyChallengeTypeLogin', this.options.authenticationId);
	}

	get isLoading(): boolean { return this.service?.status?.inProgress == true; }

	async onLoginClicked(): Promise<any> {
		await this.login();
	}

	async login(): Promise<any> {
		this.setStatusMessage('Logging in..');
		const payload: VerifyLoginRequestModel = {
			username: this.username,
			password: this.password,
			totp: this.totp
		};
		const result = await this.service.RequestAsync('VerifyLoginAsync', payload) as VerifyLoginResponseModel;
		if (result.success) {
			this.setStatusMessage('Success');
			setTimeout(() => { this.$emit('solved'); }, 1000);
		} else {
			this.setStatusMessage(result.error, true);
		}
	}

	setStatusMessage(text: string, isError: boolean = false): void {
		this.statusMessage = text;
		this.statusIsError = isError === true;
	}
}
</script>

<template>
	<div class="challenge-login">
		<div class="challenge-header">
			<div class="material-icons icon">key</div>
			<div class="challenge-title">Login</div>
		</div>

		<div v-if="options.description" class="challenge-login__description">
			<p>{{ options.description }}</p>
		</div>

		<div class="challenge-login__inputs">
			<text-input-component
				v-if="options.usernameRequired"
				v-model:value="username"
				placeholder="Username"
				:disabled="isLoading"
				@keydown.enter="onLoginClicked"
				autocomplete="username"
				/>
			<text-input-component
				v-if="options.passwordRequired"
				v-model:value="password"
				type="password"
				placeholder="Password"
				:disabled="isLoading"
				@keydown.enter="onLoginClicked"
				autocomplete="current-password"
				/>
			<text-input-component
				v-if="options.totpRequired"
				v-model:value="totp"
				placeholder="One-time code"
				:disabled="isLoading"
				@keydown.enter="onLoginClicked"
				/>

			<button-component @click="onLoginClicked" :disabled="isLoading" class="ml-0 secondary">Login</button-component>
		</div>
		
		<div v-if="statusMessage" class="challenge-login__status" :class="{ 'error': statusIsError }">
			{{ statusMessage }}
		</div>
	</div>
</template>

<style scoped lang="scss">
.challenge-login {
	&__description {
		color: var(--color--text-dark);
		text-align: center;
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
	
	.button {
		width: 100%;
    	height: 50px;
		padding: 0;
	}
}
</style>
