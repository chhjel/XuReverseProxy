<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from 'vue-property-decorator'
import VerticalLinesEffectComponent from "@components/effects/VerticalLinesEffectComponent.vue";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { LoginPageFrontendModel } from "@generated/Models/Web/LoginPageFrontendModel";
import { LoginResponse } from "@generated/Models/Web/LoginResponse";
import LoginService from "@services/LoginService";
import { CreateAccountResponse } from "@generated/Models/Web/CreateAccountResponse";
import { Ecc, QrCode } from '@utils/QRCodeUtil';
import { LoggedOutMessage_IpChanged } from "@utils/Constants";

@Options({
	components: {
		VerticalLinesEffectComponent,
		TextInputComponent,
		ButtonComponent
	}
})
export default class LoginPage extends Vue {
  	@Prop()
	options: LoginPageFrontendModel;
	
    loginService: LoginService = new LoginService();
	errorCode: string = '';

	// Login
	username: string = '';
	password: string = '';
	totp: string = '';
	loginResult: LoginResponse | null = null;
	statusMessage: string = '';
	statusIsError: boolean = false;

	// Create account
	repeatPassword: string = '';
	enableTotp: boolean = true;
	totpSecret: string = '';
    totpQrCodeIssuer: string = 'XuTunnel';
    totpQrCodeLabel: string = '';
	createAccountResult: CreateAccountResponse | null = null;

	async mounted() {
		this.totpSecret = this.options.freshTotpSecret;
		this.setErrorCodeStatus();
		this.errorCode = new URLSearchParams(window.location.search).get('err') || '';

		if (this.options.allowCreateAdmin) {
			this.totpQrCodeLabel = this.options.serverName || 'XuReverseProxy';
			this.generateQrCode();
		}
	}

	setStatus(text: string, isError?: boolean): void {
		this.statusMessage = text;
		this.statusIsError = isError == true;
	}

	setErrorCodeStatus(): void {
		if (!this.options.errorCode) return;
		else if (this.options.errorCode == 'invalid_redirect') return this.setStatus('Invalid redirect target.', true);
		else if (this.options.errorCode == 'denied') return this.setStatus('You do not have access to the requested page.', true);
		else this.setStatus(`Error #${this.options.errorCode}`, true);
	}

	async login(): Promise<LoginResponse> {
		return await this.loginService.LoginAsync(this.username, this.password, this.options.returnUrl, this.totp);
	}

	async createAccount(): Promise<CreateAccountResponse> {
		return await this.loginService.CreateAccountAsync(this.username, this.password, this.enableTotp ? this.totpSecret : '', this.totp);
	}

	async onLoginClicked(): Promise<any> {
		const validationError = this.getValidationError();
		if (validationError) {
			this.setStatus(validationError);
			return;
		}

		this.setStatus('Logging in..');
		this.loginResult = await this.login();
		if (this.loginResult.success) {
			window.location.href = this.loginResult.redirect;
			this.setStatus('Login success, redirecting..');
		} else {
			this.setStatus(this.loginResult.error || 'Invalid credentials.');
		}
	}

	async onCreateAccountClicked(): Promise<any> {
		const validationError = this.getCreateAccountValidationError();
		if (validationError) {
			this.setStatus(validationError);
			return;
		}

		this.setStatus('Creating account..');
		this.createAccountResult = await this.createAccount();
		if (this.createAccountResult.success) {
			window.location.href = this.createAccountResult.redirect;
			this.setStatus('Success, logging in..');
		} else {
			this.setStatus(this.createAccountResult.error || 'Invalid username or password.');
		}
	}

	getValidationError(): string {
		if (this.username == null || this.username.length == 0) return 'Username required.';
		else if (this.password == null || this.password.length == 0) return 'Password required.';
		return '';
	}

	getCreateAccountValidationError(): string {
		if (this.username == null || this.username.length == 0) return 'Username required.';
		else if (this.password == null || this.password.length == 0) return 'Password required.';
		else if (this.password != this.repeatPassword) return 'Passwords do not match.';
		else if (this.enableTotp && this.totpSecret.length == 0) return 'TOTP secret required.';
		else if (this.enableTotp && this.totp.length == 0) return 'TOTP code required.';
		return '';
	}

	get isLoading(): boolean { return this.loginService.status.inProgress; }

	get statusClasses(): any {
		return {
			error: this.statusIsError
		}
	}

	get errorOnPageLoad(): string {
		if (this.loginService.status.hasDoneAtLeastOnce)
			return '';
		else if (this.errorCode == 'ip_changed') 
			return LoggedOutMessage_IpChanged;
		else
			return '';
	}

    generateQrCode(): void {
        const data = this.generateTotpQrCodeData();
        const canvas = this.$refs.qrCodeCanvas as HTMLCanvasElement;
        
        const qr = QrCode.encodeText(data, Ecc.MEDIUM);
        qr.drawCanvas(10, 2, canvas);
    }

    generateTotpQrCodeData(): string {
        return `otpauth://totp/${this.totpQrCodeLabel}?secret=${this.totpSecret}&issuer=${this.totpQrCodeIssuer}`;
    }
}
</script>

<template>
	<div class="login-page">
		<vertical-lines-effect-component />

		<div class="login-form" v-if="options.isRestrictedToLocalhost">
			Admin login is restricted to localhost.
		</div>

		<div class="login-form" v-if="!options.isRestrictedToLocalhost && !options.allowCreateAdmin">
			<div class="login-form__title">Login</div>
			
			<div v-if="errorOnPageLoad" class="login-form__pageloaderror">
				{{ errorOnPageLoad }}
			</div>

			<div class="login-form__inputs">
				<text-input-component
					v-model:value="username"
					placeholder="Username"
					:disabled="isLoading"
					@keydown.enter="onLoginClicked"
					autocomplete="username"
					/>
				<text-input-component
					v-model:value="password"
					type="password"
					placeholder="Password"
					:disabled="isLoading"
					@keydown.enter="onLoginClicked"
					autocomplete="current-password"
					/>
				<text-input-component
					v-model:value="totp"
					placeholder="One-time code"
					:disabled="isLoading"
					@keydown.enter="onLoginClicked"
					/>

				<button-component @click="onLoginClicked" :disabled="isLoading">Login</button-component>
			</div>

			<div v-if="statusMessage" class="login-form__status" :class="statusClasses">
				{{ statusMessage }}
			</div>
		</div>

		<div class="login-form" v-if="!options.isRestrictedToLocalhost && options.allowCreateAdmin">
			<div class="login-form__title">Create admin account</div>

			<div class="login-form__inputs">
				<text-input-component
					v-model:value="username"
					placeholder="Username"
					:disabled="isLoading"
					@keydown.enter="onCreateAccountClicked"
					autocomplete="username"
					/>
				<text-input-component
					v-model:value="password"
					type="password"
					placeholder="Password"
					:disabled="isLoading"
					@keydown.enter="onCreateAccountClicked"
					autocomplete="new-password"
					/>
				<text-input-component
					v-model:value="repeatPassword"
					type="password"
					placeholder="Repeat password"
					:disabled="isLoading"
					@keydown.enter="onCreateAccountClicked"
					autocomplete="new-password"
					/>

				<div class="pt-2 pb-2">
					<label for="enableTotp">Enable two-factor authentication</label>
					<input id="enableTotp" type="checkbox" v-model="enableTotp" />
				</div>

				<div class="login-form__totp">
					<div v-show="enableTotp">
						<p>Scan the QR code with an authenticator app</p>
						<canvas ref="qrCodeCanvas"></canvas>
						<p v-if="totpSecret">Or optionally enter this secret manually in your app of choice: <code>{{ totpSecret }}</code></p>
						<text-input-component
							v-show="enableTotp"
							v-model:value="totp"
							placeholder="TOTP code"
							:disabled="isLoading"
							@keydown.enter="onCreateAccountClicked"
							/>
					</div>
				</div>

				<button-component @click="onCreateAccountClicked" :disabled="isLoading">Create account</button-component>
			</div>

			<div v-if="statusMessage" class="login-form__status" :class="statusClasses">
				{{ statusMessage }}
			</div>
		</div>
	</div>
</template>

<style scoped lang="scss">
.login-form {
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);

    padding: 20px;
	padding-bottom: 10px;
    width: calc(min(100vw, 500px));
	box-sizing: border-box;

    background-color: var(--color--panel);
    border: 1px solid #3f3f3f;

    display: flex;
    flex-direction: column;
    text-align: center;

	canvas {
		max-width: 100%;
	}

	.input-wrapper {
		margin-bottom: 4px;
	}
		
	.button {
		margin-top: 15px;
	}

	&__title {
		font-size: 28px;
		text-transform: uppercase;
	}
	
	&__inputs {
		margin-top: 20px;
	}

	&__pageloaderror {
		color: var(--color--warning-base);
		border: 1px solid var(--color--warning-base);
		padding: 10px 5px;
		margin-top: 15px;
		margin-bottom: 5px;
		font-size: 16px;
	}

	&__status {
		margin-top: 10px;
		margin-bottom: 5px;
		&.error {
			color: var(--color--error-base);
		}
	}

	&__alreadyloggedin {
		margin-top: 10px;
		margin-bottom: 5px;
	}
}
</style>
