<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from "vue-property-decorator";
import InputHeaderComponent from "./InputHeaderComponent.vue";
import ButtonComponent from "./ButtonComponent.vue";
import { format } from "date-fns";

@Options({
  components: {
    InputHeaderComponent,
    ButtonComponent,
  },
})
export default class DateTimeInputComponent extends Vue {
  @Prop({ required: true })
  value!: string | null;

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

  localValue: string | null = null;

  // from backend: 2023-09-20T16:05:00Z

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

  formatDateForInput(date: Date): string {
    return `${format(date, "yyyy-MM-dd")}T${format(date, "HH:mm")}`;
  }

  formatDateForOutput(value: string): string {
    let localDate = new Date(value);
    return localDate.toISOString();
  }

  clear(): void {
    this.localValue = null;
  }

  /////////////////
  //  WATCHERS  //
  ///////////////
  @Watch("value")
  updateLocalValue(): void {
    if (!this.value) {
      this.localValue = null;
    } else {
      this.localValue = this.formatDateForInput(new Date(this.value));
    }
  }

  @Watch("localValue")
  emitLocalValue(): void {
    if (this.disabled) {
      this.updateLocalValue();
      return;
    }

    let valueToEmit: string | null = null;
    if (this.emptyIsNull && `${this.localValue}` === "") {
      valueToEmit = null;
    } else {
      valueToEmit = this.formatDateForOutput(this.localValue);
    }

    this.$emit("update:value", valueToEmit);
    this.$emit("change", valueToEmit);
  }
}
</script>

<template>
  <div class="input-wrapper" :class="wrapperClasses">
    <input-header-component :label="label" :description="description" />

    <div class="datetime__inputs">
      <div class="datetime__input-wrapper">
        <input
          type="datetime-local"
          v-model="localValue"
          :disabled="disabled"
        />
      </div>

      <div class="datetime__clear">
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

    <div class="datetime__note">
      <code v-if="value === null && noteIfNull">{{ noteIfNull }}</code>
      <code v-if="value !== null && noteIfNotNull">{{ noteIfNotNull }}</code>
    </div>
  </div>
</template>

<style scoped lang="scss">
.datetime__inputs {
  display: flex;
  flex-wrap: nowrap;
}
.datetime__input-wrapper {
  width: 70px;
  margin-right: 4px;
  color: var(--color--text-dark);

  input {
    width: 300px;
  }
}
.datetime__clear {
  margin-top: 22px;
}
</style>
