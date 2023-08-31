<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Prop, Watch } from 'vue-property-decorator'
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import { ProxyChallengeTypeLogin } from "@generated/Models/Core/ProxyChallengeTypeLogin";

@Options({
	components: {
		TextInputComponent,
		ButtonComponent
	}
})
export default class ProxyChallengeTypeLoginEditor extends Vue {
  	@Prop()
	value: string;

  	@Prop({ required: false, default: false})
	disabled: boolean;
	
	localValue: ProxyChallengeTypeLogin | null= null;

    mounted(): void {
        this.updateLocalValue();
        this.emitLocalValue();
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
	<div class="proxy-challenge-login-edit" v-if="localValue">
		<div>Use admin login: <code>{{ localValue.useIdentity }}</code> // todo cb</div>
		<text-input-component label="Description" v-model:value="localValue.description" />
		<text-input-component label="Username" v-model:value="localValue.username" v-if="!localValue.useIdentity" />
		<text-input-component label="Password" v-model:value="localValue.password" v-if="!localValue.useIdentity" />
		<text-input-component label="TOTP Secret" v-model:value="localValue.totpSecret" v-if="!localValue.useIdentity" />
	</div>
</template>

<style scoped lang="scss">
.proxy-challenge-login-edit {

}
</style>
