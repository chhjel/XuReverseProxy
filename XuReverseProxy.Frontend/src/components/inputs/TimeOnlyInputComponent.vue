<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from "vue-property-decorator";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ButtonComponent from "./ButtonComponent.vue";

@Options({
  components: {
    InputHeaderComponent,
    ButtonComponent,
  },
})
export default class TimeOnlyInputComponent extends Vue {
  @Prop({ required: true })
  value!: string;

  @Prop({ required: false, default: "" })
  label: string;

  @Prop({ required: false, default: "" })
  description: string;

  @Prop({ required: false, default: "" })
  noteIfNull: string;

  @Prop({ required: false, default: "" })
  noteIfNotNull: string;

  @Prop({ required: false, default: false })
  disabled: boolean;

  @Prop({ required: false, default: false })
  emptyIsNull: boolean;

  placeholder: string = "00";

  localValueHours: string = "";
  localValueMinutes: string = "";
  localValueSeconds: string = "";

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  get wrapperClasses(): any {
    let classes: any = {
      disabled: this.disabled,
    };
    return classes;
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    if (!this.value) {
      this.localValueHours = "";
      this.localValueMinutes = "";
      this.localValueSeconds = "";
    } else if (this.value.includes(".")) {
      let hmsParts = this.value.split(":");
      this.localValueHours = isNaN(parseInt(hmsParts[0])) ? "0" : parseInt(hmsParts[0]).toString() || "0";
      this.localValueMinutes = isNaN(parseInt(hmsParts[1])) ? "0" : parseInt(hmsParts[1]).toString() || "0";
      this.localValueSeconds = isNaN(parseInt(hmsParts[2])) ? "0" : parseInt(hmsParts[2]).toString() || "0";
    }
  }

  @Watch("localValueHours")
  @Watch("localValueMinutes")
  @Watch("localValueSeconds")
  emitLocalValue(): void {
    if (this.disabled) {
      this.updateLocalValue();
      return;
    }

    this.validateInput(
      () => this.localValueHours,
      (v) => (this.localValueHours = v),
      23,
    );
    this.validateInput(
      () => this.localValueMinutes,
      (v) => (this.localValueMinutes = v),
      59,
    );
    this.validateInput(
      () => this.localValueSeconds,
      (v) => (this.localValueSeconds = v),
      59,
    );

    let valueToEmit: string | null = null;
    if (
      this.emptyIsNull &&
      `${this.localValueHours}${this.localValueMinutes}${this.localValueSeconds}` === ""
    ) {
      valueToEmit = null;
    } else {
      let hours = (this.localValueHours || "0").padStart(2, "0");
      let minutes = (this.localValueMinutes || "0").padStart(2, "0");
      let seconds = (this.localValueSeconds || "0").padStart(2, "0");
      valueToEmit = `${hours}:${minutes}:${seconds}`;
    }

    this.$emit("update:value", valueToEmit);
    this.$emit("change", valueToEmit);
  }

  validateInput(getter: () => string, setter: (val: string) => void, maxValue: number): void {
    let value = getter();
    if (value.length == 0) return;

    const intValue = parseInt(value);
    if (isNaN(intValue)) value = "";
    else if (intValue > maxValue) value = maxValue.toString();

    setter(value);
  }

  clear(): void {
    this.localValueHours = "";
    this.localValueMinutes = "";
    this.localValueSeconds = "";
  }
}
</script>

<template>
  <div class="input-wrapper" :class="wrapperClasses">
    <input-header-component :label="label" :description="description" />

    <div class="timeonly__inputs">
      <div class="timeonly__input-wrapper">
        <div>Hour</div>
        <input type="text" v-model="localValueHours" :disabled="disabled" :placeholder="placeholder" />
      </div>
      <div class="timeonly__input-wrapper">
        <div>Minute</div>
        <input type="text" v-model="localValueMinutes" :disabled="disabled" :placeholder="placeholder" />
      </div>
      <div class="timeonly__input-wrapper">
        <div>Second</div>
        <input type="text" v-model="localValueSeconds" :disabled="disabled" :placeholder="placeholder" />
      </div>

      <div class="timeonly__clear">
        <button-component
          icon="close"
          :disabled="disabled"
          v-if="emptyIsNull"
          title="Clear"
          iconOnly
          secondary
          @click="clear"
          class="ml-2"
        ></button-component>
      </div>
    </div>

    <div class="timeonly__note">
      <code v-if="value === null && noteIfNull">{{ noteIfNull }}</code>
      <code v-if="value !== null && noteIfNotNull">{{ noteIfNotNull }}</code>
    </div>
  </div>
</template>

<style scoped lang="scss">
.timeonly__inputs {
  display: flex;
  flex-wrap: nowrap;
}
.timeonly__input-wrapper {
  width: 70px;
  margin-right: 4px;
  color: var(--color--text-dark);

  input {
    width: 45px;
  }
}
.timeonly__clear {
  margin-top: 22px;
}
</style>
