<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeAdminLoginFrontendModel } from "@generated/Models/Core/ProxyChallengeTypeAdminLoginFrontendModel";
import { VerifyLoginRequestModel } from "@generated/Models/Core/VerifyLoginRequestModel";
import ProxyAuthService from "@services/ProxyAuthService";
import { VerifyLoginResponseModel } from "@generated/Models/Core/VerifyLoginResponseModel";
import TOTPTimebarComponent from "@components/common/TOTPTimebarComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    TOTPTimebarComponent,
  },
})
export default class ProxyChallengeTypeAdminLoginComponent extends Vue {
  @Prop()
  options: ProxyChallengeTypeAdminLoginFrontendModel;

  service: ProxyAuthService = new ProxyAuthService("ProxyChallengeTypeAdminLogin", "");
  username: string = "";
  password: string = "";
  totp: string = "";
  loginResult: boolean | null = null;

  statusMessage: string = "";
  statusIsError: boolean = false;

  async mounted() {
    this.service = new ProxyAuthService("ProxyChallengeTypeAdminLogin", this.options.authenticationId);
  }

  get isLoading(): boolean {
    return this.service?.status?.inProgress == true;
  }

  async onLoginClicked(): Promise<any> {
    await this.login();
  }

  async login(): Promise<any> {
    this.setStatusMessage("Logging in..");
    const payload: VerifyLoginRequestModel = {
      username: this.username,
      password: this.password,
      totp: this.totp,
    };
    const result = (await this.service.RequestAsync("VerifyLoginAsync", payload)) as VerifyLoginResponseModel;
    if (result.success) {
      this.setStatusMessage("Success");
      setTimeout(() => {
        this.$emit("solved");
      }, 1000);
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
  <div class="challenge-admin-login">
    <div class="challenge-header">
      <div class="material-icons icon">key</div>
      <div class="challenge-title">Admin login</div>
    </div>

    <div v-if="options.description" class="challenge-admin-login__description">
      <p>{{ options.description }}</p>
    </div>

    <div class="challenge-admin-login__inputs">
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
      <TOTPTimebarComponent class="mb-1" />

      <button-component @click="onLoginClicked" :disabled="isLoading" class="ml-0 secondary">Login</button-component>
    </div>

    <div v-if="statusMessage" class="challenge-admin-login__status" :class="{ error: statusIsError }">
      {{ statusMessage }}
    </div>
  </div>
</template>

<style scoped lang="scss">
.challenge-admin-login {
  &__description {
    color: var(--color--text-dark);
    text-align: center;
  }

  &__inputs {
    max-width: 300px;
    margin: 20px auto 0 auto;

    .input-wrapper {
      margin-bottom: 4px;
    }
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
