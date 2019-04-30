using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPool.NET.Configuration
{
    public  class PoolSetting : ConfigurationSection
    {
        [ConfigurationProperty(PoolSettingConstants.LIMIT_LIFE, IsRequired = false, DefaultValue =60)]
        public int LimitLife
        {
            get
            {
                return (int)this[PoolSettingConstants.LIMIT_LIFE];
            }
            set
            {
                this[PoolSettingConstants.LIMIT_LIFE]= value;
            }
        }
        [ConfigurationProperty(PoolSettingConstants.LIMIT_TIMES, IsRequired = false, DefaultValue = 100)]
        public int LimitTimes
        {
            get
            {
                return (int)this[PoolSettingConstants.LIMIT_TIMES];
            }
            set
            {
                this[PoolSettingConstants.LIMIT_TIMES] = value;
            }
        }

        [ConfigurationProperty(PoolSettingConstants.MAX_POOL_SIZE, IsRequired = false, DefaultValue = 10)]
        public int MaxPoolSize
        {
            get
            {
                return (int)this[PoolSettingConstants.MAX_POOL_SIZE];
            }
            set
            {
                this[PoolSettingConstants.MAX_POOL_SIZE] = value;
            }
        }

        [ConfigurationProperty(PoolSettingConstants.MIN_POOL_SIZE, IsRequired = false, DefaultValue = 3)]
        public int MinPoolSize
        {
            get
            {
                return (int)this[PoolSettingConstants.MIN_POOL_SIZE];
            }
            set
            {
                this[PoolSettingConstants.MIN_POOL_SIZE] = value;
            }
        }

        [ConfigurationProperty(PoolSettingConstants.WORKING_DIRECTORY, IsRequired = true)]
        public string WorkingDirectory
        {
            get
            {
                return (string)this[PoolSettingConstants.WORKING_DIRECTORY];
            }
            set
            {
                this[PoolSettingConstants.WORKING_DIRECTORY] = value;
            }
        }

        [ConfigurationProperty(PoolSettingConstants.R_DIRECTORY, IsRequired = false,DefaultValue ="R")]
        public string RDirectory
        {
            get
            {
                return (string)this[PoolSettingConstants.R_DIRECTORY];
            }
            set
            {
                this[PoolSettingConstants.R_DIRECTORY] = value;
            }
        }
    }
}
