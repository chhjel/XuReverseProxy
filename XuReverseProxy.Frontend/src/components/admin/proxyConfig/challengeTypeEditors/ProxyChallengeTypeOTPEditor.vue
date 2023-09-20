<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeOTP } from "@generated/Models/Core/ProxyChallengeTypeOTP";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
import { PlaceholderInfo, PlaceholderGroupInfo, OTPRequestUrlPlaceholders } from "@utils/Constants";
import CodeInputComponent from "@components/inputs/CodeInputComponent.vue";
import CustomRequestDataEditor from "@components/inputs/CustomRequestDataEditor.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    CodeInputComponent,
    ExpandableComponent,
    PlaceholderInfoComponent,
    CustomRequestDataEditor,
  },
})
export default class ProxyChallengeTypeOTPEditor extends Vue {
  @Prop()
  value: string;

  @Prop({ required: false, default: false })
  disabled: boolean;

  localValue: ProxyChallengeTypeOTP | null = null;

  placeholdersExtra: Array<PlaceholderInfo> = [
    {
      name: "code",
      description: "Generated one-time code.",
    },
  ];
  placeholders: Array<PlaceholderGroupInfo> = OTPRequestUrlPlaceholders;

  mounted(): void {
    this.updateLocalValue();
    if (!this.localValue.requestData)
      this.localValue.requestData = {
        url: "",
        body: "",
        headers: "",
        requestMethod: "",
      };
    if (!this.localValue.requestData.requestMethod) this.localValue.requestData.requestMethod = "GET";
    if (!this.localValue.requestData.url)
      this.localValue.requestData.url = "https://www.your-otp-service.com?code={{code}}";
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
}
</script>

<template>
  <div class="proxy-challenge-otp-edit" v-if="localValue">
    <p>When the user clicks the button to send a one-time code a request is sent to the webhook url.</p>
    <text-input-component label="Description" v-model:value="localValue.description" :disabled="disabled" />
    <CustomRequestDataEditor
      v-if="localValue"
      v-model:value="localValue.requestData"
      :placeholders="placeholders"
      :additionalPlaceholders="placeholdersExtra"
    />
  </div>
</template>

<style scoped lang="scss">
/* .proxy-challenge-otp-edit { } */
</style>
