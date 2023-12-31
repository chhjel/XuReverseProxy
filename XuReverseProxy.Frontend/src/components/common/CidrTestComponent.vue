<script lang="ts">
import { Vue, Prop } from "vue-property-decorator";
import { Options } from "vue-class-component";
import MiscUtilsService from "@services/MiscUtilsService";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import LoaderComponent from "./LoaderComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    LoaderComponent,
  },
})
export default class CidrTestComponent extends Vue {
  @Prop()
  value: string;

  service: MiscUtilsService = new MiscUtilsService();
  input: string = "";
  testResult: string = "";
  testResultClasses: any = {};
  success: boolean | null = null;

  mounted(): void {}

  get isLoading(): boolean {
    return this.service.status.inProgress;
  }

  async test() {
    this.success = await this.service.IsIPInCidrRangeAsync({
      ip: this.input,
      ipCidr: this.value,
    });
    this.testResult = this.success ? "IP is within CIDR range." : "IP is not within CIDR range.";
    this.testResultClasses["success"] = this.success;
  }
}
</script>

<template>
  <div class="cidr-test">
    <text-input-component label="Value to test CIDR range against" v-model:value="input" :disabled="isLoading" />
    <button-component @click="test" :disabled="isLoading" class="ml-0 mr-1" secondary>Test CIDR</button-component>
    <div class="cidr-test-result" :class="testResultClasses" v-if="testResult">
      {{ testResult }}
    </div>
    <loader-component :status="service.status" inline inlineYAdjustment="-4px" />
  </div>
</template>

<style scoped lang="scss">
.cidr-test {
  display: block;

  &-result {
    font-family: monospace;
    padding: 10px;
    font-weight: 600;
    color: var(--color--warning-base);
    &.success {
      color: var(--color--success-base);
    }
  }
}
</style>
