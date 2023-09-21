<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeLogin } from "@generated/Models/Core/ProxyChallengeTypeLogin";
import Base32UtilService from "@services/Base32UtilService";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import { LoginUsernamePasswordPlaceholders, PlaceholderGroupInfo } from "@utils/Constants";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    ExpandableComponent,
    PlaceholderInfoComponent,
  },
})
export default class ProxyChallengeTypeLoginEditor extends Vue {
  @Prop()
  value: string;

  @Prop({ required: false, default: false })
  disabled: boolean;

  localValue: ProxyChallengeTypeLogin | null = null;

  base32Service: Base32UtilService = new Base32UtilService();
  placeholders: Array<PlaceholderGroupInfo> = LoginUsernamePasswordPlaceholders;
  @Ref() readonly usernameInput!: any;
  @Ref() readonly passwordInput!: any;

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    const changed = JSON.stringify(this.localValue) != this.value;
    if (changed) this.localValue = JSON.parse(this.value);
  }

  @Watch("localValue", { deep: true })
  emitLocalValue(): void {
    if (this.disabled) {
      this.updateLocalValue();
      return;
    }

    this.$emit("update:value", JSON.stringify(this.localValue));
  }

  async generateSecret() {
    const result = await this.base32Service.CreateSecretAsync();
    this.localValue.totpSecret = result;
  }

  placeholderTarget: "user" | "password" | "" = "";
  insertPlaceholder(val: string): void {
    if (this.placeholderTarget == "user") this.usernameInput.insertText(val);
    else if (this.placeholderTarget == "password") this.passwordInput.insertText(val);
  }
}
</script>

<template>
  <div class="proxy-challenge-login-edit" v-if="localValue">
    <p>
      Requires the user to login with the set details. All the fields are optional, you can use e.g. only username +
      TOTP if wanted.
    </p>
    <text-input-component label="Description" v-model:value="localValue.description" :disabled="disabled" />
    <text-input-component
      label="Username"
      v-model:value="localValue.username"
      :disabled="disabled"
      @focus="placeholderTarget = 'user'"
      ref="usernameInput"
      class="mt-2"
    />
    <text-input-component
      label="Password"
      v-model:value="localValue.password"
      :disabled="disabled"
      @focus="placeholderTarget = 'password'"
      ref="passwordInput"
      class="mt-2"
    />

    <expandable-component header="Supported placeholders for username & password">
      <placeholder-info-component :placeholders="placeholders" @insertPlaceholder="insertPlaceholder" />
    </expandable-component>

    <text-input-component label="TOTP Secret" v-model:value="localValue.totpSecret" :disabled="disabled" class="mt-2" />
    <div @click="generateSecret" style="cursor: pointer">[generate secret]</div>
  </div>
</template>

<style scoped lang="scss">
/* .proxy-challenge-login-edit { } */
</style>
