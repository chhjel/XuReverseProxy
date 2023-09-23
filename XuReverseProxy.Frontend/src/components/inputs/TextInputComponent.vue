<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Ref, Vue, Watch } from "vue-property-decorator";
import ProgressbarComponent from "@components/common/ProgressbarComponent.vue";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ValueUtils from "@utils/ValueUtils";

@Options({
  components: {
    ProgressbarComponent,
    InputHeaderComponent,
  },
})
export default class TextInputComponent extends Vue {
  @Prop({ required: true })
  value!: string;

  @Prop({ required: false, default: "text" })
  type: string;

  @Prop({ required: false, default: "" })
  placeholder: string;

  @Prop({ required: false, default: "" })
  label: string;

  @Prop({ required: false, default: "" })
  description: string;

  @Prop({ required: false, default: "" })
  autocomplete: string;

  @Prop({ required: false, default: false })
  disabled: boolean;

  @Prop({ required: false, default: false })
  emptyIsNull: boolean;

  @Prop({ required: false, default: null })
  progress: number | null;

  @Prop({ required: false, default: "var(--color--primary)" })
  progressColor: string;

  @Prop({ required: false, default: false })
  dark: boolean;

  @Prop({ required: false, default: false })
  textarea: boolean;

  @Prop({ required: false, default: null })
  rows: number | null;

  @Ref() readonly inputElement!: HTMLInputElement;
  @Ref() readonly textAreaElement!: HTMLTextAreaElement;
  localValue: string = "";

  mounted(): void {
    this.updateLocalValue();
    this.emitLocalValue(["update:value"]);
  }

  get wrapperClasses(): any {
    let classes: any = {
      disabled: this.disabled,
      dark: this.isDark,
    };
    return classes;
  }

  get isTextArea(): boolean {
    return ValueUtils.IsToggleTrue(this.textarea);
  }
  get isDark(): boolean {
    return ValueUtils.IsToggleTrue(this.dark);
  }

  get showProgress(): boolean {
    return this.progress !== null;
  }

  onFocus(): void {
    this.$emit("focus");
  }

  onBlur(): void {
    this.$emit("blur");
  }

  onChange(): void {
    this.emitLocalValue(["change"]);
  }

  public insertText(val: string): void {
    const el = this.isTextArea ? this.textAreaElement : this.inputElement;
    const [start, end] = [el.selectionStart, el.selectionEnd];
    el.setRangeText(val, start, end, "select");
    this.localValue = el.value;
    this.emitLocalValue(["update:value", "change"]);
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    this.localValue = this.value;
  }

  @Watch("localValue")
  onLocalValueChanged(): void {
    this.emitLocalValue(["update:value"]);
  }

  emitLocalValue(types: Array<string>): void {
    if (this.disabled) {
      this.localValue = this.value;
      return;
    }

    let valueToEmit: string | number | null = this.localValue;

    if (this.emptyIsNull && valueToEmit === "") {
      valueToEmit = null;
    }

    types.forEach((x) => this.$emit(x, valueToEmit));
  }
}
</script>

<template>
  <div class="input-wrapper" :class="wrapperClasses">
    <input-header-component :label="label" :description="description" />

    <input
      v-if="!isTextArea"
      :type="type"
      v-model="localValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :autocomplete="autocomplete"
      @focus="onFocus"
      @blur="onBlur"
      @change="onChange"
      ref="inputElement"
    />

    <textarea
      v-if="isTextArea"
      v-model="localValue"
      :placeholder="placeholder"
      :disabled="disabled"
      :autocomplete="autocomplete"
      @focus="onFocus"
      @blur="onBlur"
      @change="onChange"
      :rows="rows"
      ref="textAreaElement"
    ></textarea>

    <progressbar-component v-if="showProgress" :value="progress" :color="progressColor" />
  </div>
</template>

<style scoped lang="scss"></style>
