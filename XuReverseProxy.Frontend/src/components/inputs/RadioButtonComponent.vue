<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from "vue-property-decorator";
import ProgressbarComponent from "@components/common/ProgressbarComponent.vue";
import IdUtils from "@utils/IdUtils";
import ValueUtils from "@utils/ValueUtils";
import { RadioButtonOption } from "./RadioButtonComponent.Models";

@Options({
  components: { ProgressbarComponent },
})
export default class RadioButtonComponent extends Vue {
  @Prop({ required: false, default: "" })
  label!: string;

  @Prop({ required: true })
  value!: string;

  @Prop({ required: true })
  options!: Array<RadioButtonOption>;

  @Prop({ required: false, default: false })
  disabled: boolean;

  localValue: string = "";

  id: string = IdUtils.generateId();

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue();
  }

  get wrapperClasses(): any {
    let classes: any = {
      disabled: this.isDisabled,
    };
    return classes;
  }

  get isDisabled(): boolean {
    return ValueUtils.IsToggleTrue(this.disabled);
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    this.localValue = this.value;
  }

  @Watch("localValue")
  emitLocalValue(): void {
    if (this.isDisabled) {
      this.localValue = this.value;
      return;
    }

    this.$emit("update:value", this.localValue);
    this.$emit("change", this.localValue);
  }
}
</script>

<template>
  <div :class="wrapperClasses" class="radio-buttons-component">
    <div v-if="label" class="label">{{ label }}</div>
    <div class="radio-buttons">
      <label v-for="option in options" :key="`${id}-${option.value}`" tabindex="0">
        <input type="radio" :name="`rb-${id}`" :value="option.value" :disabled="isDisabled" v-model="localValue" />
        <span>{{ option.label }}</span>
      </label>
    </div>
  </div>
</template>

<style scoped lang="scss">
.radio-buttons-component {
  display: flex;
  align-items: center;

  .label {
    font-size: 22px;
    color: var(--color--text-dark);
    margin-right: 10px;
  }
}

.radio-buttons {
  display: flex;
  align-items: center;
  flex-wrap: wrap;

  label {
    display: flex;
    align-items: center;
    margin-right: 15px;

    span {
      margin-left: 5px;
    }
  }

  input[type="radio"] {
    box-sizing: border-box;
    appearance: none;
    background: var(--color--panel-light);
    border: 2px solid var(--color--dialog);
    width: 24px;
    height: 24px;
    margin: 0;
  }

  input[type="radio"]:checked {
    background: var(--color--success-base);
    border-color: var(--color--panel-light);
  }

  input[type="radio"]:checked:disabled {
    background: var(--color--primary);
  }
}
</style>
