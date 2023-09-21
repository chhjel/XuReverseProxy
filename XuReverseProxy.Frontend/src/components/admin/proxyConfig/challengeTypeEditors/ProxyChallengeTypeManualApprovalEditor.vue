<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch, Ref } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeManualApproval } from "@generated/Models/Core/ProxyChallengeTypeManualApproval";
import { ManualApprovalUrlPlaceholders, PlaceholderGroupInfo, PlaceholderInfo } from "@utils/Constants";
import ExpandableComponent from "@components/common/ExpandableComponent.vue";
import PlaceholderInfoComponent from "@components/common/PlaceholderInfoComponent.vue";
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
export default class ProxyChallengeTypeManualApprovalEditor extends Vue {
  @Prop()
  value: string;

  @Prop({ required: false, default: false })
  disabled: boolean;

  localValue: ProxyChallengeTypeManualApproval | null = null;

  placeholdersExtra: Array<PlaceholderInfo> = [
    {
      name: "url",
      description: "The generated, escaped url of the page where you can approve the request.",
    },
  ];
  placeholders: Array<PlaceholderGroupInfo> = ManualApprovalUrlPlaceholders;

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
      this.localValue.requestData.url = "https://www.your-notification-service.com?url={{url}}";
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
  <div class="proxy-challenge-manual-edit" v-if="localValue">
    <p>When the user clicks the button to request access, a request is sent to the webhook url.</p>
    <CustomRequestDataEditor
      v-if="localValue"
      v-model:value="localValue.requestData"
      :placeholders="placeholders"
      :additionalPlaceholders="placeholdersExtra"
    />
  </div>
</template>

<style scoped lang="scss">
/* .proxy-challenge-manual-edit { } */
</style>
