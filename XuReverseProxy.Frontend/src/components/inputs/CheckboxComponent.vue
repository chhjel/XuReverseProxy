<script lang="ts">
import { Options } from "vue-class-component";
import { Prop, Vue, Watch } from 'vue-property-decorator'
import ProgressbarComponent from "@components/common/ProgressbarComponent.vue";
import IdUtils from "@utils/IdUtils";
import ValueUtils from "@utils/ValueUtils";

@Options({
	components: { ProgressbarComponent }
})
export default class CheckboxComponent extends Vue {
    @Prop({ required: true })
    value!: boolean;

    @Prop({ required: false, default: '' })
    label: string;

    @Prop({ required: false, default: '' })
    offLabel: string;

    @Prop({ required: false, default: false })
    disabled: boolean;

    @Prop({ required: false, default: false })
    warnColorOff: boolean;
    
    localValue: boolean = false;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    get wrapperClasses(): any {
        let classes: any = {
            disabled: this.isDisabled,
            warn: !this.localValue && this.isWarnColorOff
        };
        return classes;
    }

    get isDisabled(): boolean { return ValueUtils.IsToggleTrue(this.disabled); }
    get isWarnColorOff(): boolean { return ValueUtils.IsToggleTrue(this.warnColorOff); }

    get currentLabel(): string {
      if (!this.offLabel) return this.label;
      else if (!this.localValue) return this.offLabel;
      else return this.label;
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
        if (this.isDisabled) {
            this.localValue = this.value;
            return;
        }

		this.$emit('update:value', this.localValue);
		this.$emit('change', this.localValue);
    }
}
</script>

<template>
  <div :class="wrapperClasses">
    <label>
      <input type="checkbox" v-model="localValue" :disabled="isDisabled">
      <div class="toggler-slider">
        <div class="toggler-knob"></div>
      </div>
      <div class="label-text">{{ currentLabel }}</div>
    </label>
  </div>
</template>

<style scoped lang="scss">
.label-text {
  margin-left: 10px;
  color: var(--color--text);
}

label {
  display: flex;
  align-items: center;
	cursor: pointer;
	position: relative;
}

label input[type="checkbox"] {
	display: none;
}

label input[type="checkbox"]:checked+.toggler-slider {
	background-color: var(--color--success-base);
}

label .toggler-slider {
	height: 25px;
	background-color: #ccc;
	position: relative;
	border-radius: 100px;
	top: 0;
	left: 0;
	width: 100%;
	-webkit-transition: all 300ms ease;
	transition: all 300ms ease;
}

label .toggler-knob {
	position: absolute;
	-webkit-transition: all 300ms ease;
	transition: all 300ms ease;
}

label input[type="checkbox"]:checked+.toggler-slider {
	background-color: var(--color--dialog);
	border-color: var(--color--success-base);
}

label input[type="checkbox"]:checked+.toggler-slider .toggler-knob {
	left: calc(100% - 19px - 3px);
	background-color: var(--color--success-base);
}

.disabled {
  .toggler-slider {
    border-color: var(--color--panel-light) !important;
  }
  .toggler-knob {
    background-color: var(--color--panel-light) !important;;
  }
}

label .toggler-slider {
  width: 42px;
	background-color: transparent;
	border-radius: 0;
	border: 2px solid #b3b3b3;
}

label .toggler-slider:after {
	position: absolute;
	top: 50%;
	right: -30px;
	-webkit-transform: translateY(-50%);
	transform: translateY(-50%);
	font-size: 75%;
	text-transform: uppercase;
	font-weight: 500;
	opacity: 0.7;
}

label .toggler-knob {
	width: 19px;
	height: 19px;
	left: 3px;
	top: 3px;
	background-color: #b3b3b3;
}

.warn {
  label input[type="checkbox"]+.toggler-slider {
	  background-color: var(--color--dialog);
    border-color: var(--color--warning-base);
  }
  label input[type="checkbox"]+.toggler-slider .toggler-knob {
    background-color: var(--color--warning-base);
  }
}
</style>
