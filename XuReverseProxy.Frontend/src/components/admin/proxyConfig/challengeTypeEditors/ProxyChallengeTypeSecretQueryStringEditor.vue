<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeSecretQueryString } from "@generated/Models/Core/ProxyChallengeTypeSecretQueryString";
import IdUtils from "@utils/IdUtils";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeSecretQueryStringEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeSecretQueryString | null= null;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
    }

    generateNewSecret(): void {
        this.localValue.secret = IdUtils.generateId();
    }

    /////////////////
    //  WATCHERS  //
    ///////////////
    @Watch('value')
    updateLocalValue(): void
    {
		const changed = JSON.stringify(this.localValue) != this.value;
		if (changed) this.localValue = JSON.parse(this.value);
    }

    @Watch('localValue', { deep: true })
    emitLocalValue(): void
    {
        if (this.disabled) {
            this.updateLocalValue();
            return;
        }

		this.$emit('update:value', JSON.stringify(this.localValue));
    }
}
</script>

<template>
	<div class="proxy-challenge-secretqs-edit" v-if="localValue">
        <p>Requires the configured secret in the url: <code>?secret={{ localValue.secret }}</code>.</p>
		<text-input-component label="Secret" v-model:value="localValue.secret" />
        <span @click="generateNewSecret" style="cursor: pointer;">[generate secret]</span>
	</div>
</template>

<style scoped lang="scss">
.proxy-challenge-secretqs-edit {

}
</style>
