<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Ref, Vue, Watch } from 'vue-property-decorator'
import ProgressbarComponent from "@components/common/ProgressbarComponent.vue";

@Options({
	components: { ProgressbarComponent }
})
export default class TextInputComponent extends Vue {
    @Prop({ required: true })
    value!: string;

    @Prop({ required: false, default: 'text' })
    type: string;

    @Prop({ required: false, default: '' })
    placeholder: string;

    @Prop({ required: false, default: '' })
    label: string;

    @Prop({ required: false, default: false })
    disabled: boolean;

    @Prop({ required: false, default: false })
    emptyIsNull: boolean;

    @Prop({ required: false, default: null })
    progress: number | null;

    @Prop({ required: false, default: 'var(--color--primary)' })
    progressColor: string;

    @Ref() readonly inputElement!: HTMLInputElement;
    localValue: string = "";

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    get wrapperClasses(): any {
        let classes: any = {
            disabled: this.disabled
        };
        return classes;
    }

    get showProgress(): boolean {
        return this.progress !== null;
    }
    
    onFocus(): void {
        this.$emit('focus');
    }

    public insertText(val: string): void
    {
        const [start, end] = [this.inputElement.selectionStart, this.inputElement.selectionEnd];
        this.inputElement.setRangeText(val, start, end, 'select');
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		this.localValue = this.value;
    }

    @Watch('localValue')
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.localValue = this.value;
            return;
        }

        let valueToEmit: string | number | null = this.localValue;

        if (this.emptyIsNull && valueToEmit === '') {
            valueToEmit = null;
        }
        // else if (this.type == 'number' && valueToEmit !== null && typeof valueToEmit == 'string') {
        //     // console.log(JSON.stringify(valueToEmit));
        //     valueToEmit = parseInt(valueToEmit);
        // }

		this.$emit('update:value', valueToEmit);
		this.$emit('change', valueToEmit);
    }
}
</script>

<template>
  <div class="input-wrapper" :class="wrapperClasses">
    <label v-if="label">{{ label }}</label>

    <input
        :type="type"
        v-model="localValue"
        :placeholder="placeholder"
        :disabled="disabled"
        @focus="onFocus"
        ref="inputElement" />

    <progressbar-component
        v-if="showProgress"
        :value="progress"
        :color="progressColor"
        />
  </div>
</template>

<style scoped lang="scss">
</style>
